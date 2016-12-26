using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Services;
using Newtonsoft.Json;
using System.Net;
using System.Xml;

namespace WebApplication1
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        public static CBR t;

        protected void Page_Init(object sender, EventArgs e)
        {
            t = new CBR();  //새로운 CBR 알고리즘 객체 생성
            t.Loadlog();  //Mysql에 접속해서 DB를 불러온다.
            t.AddConversation("안녕", "네, 안녕하세요!");   //질문-답 학습시키는 메소드(AddConversation)
            t.AddConversation("하이", "HI~!!");
            t.AddConversation("고마워", "별 말씀을요~");
     
        }
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        [WebMethod]
        public static string def_Function(String data)     //네이버 검색 API를 통해 정의를 가져올 경우
        {
            try
            {
                String LoadedXML;

                WebClient wc = new WebClient();

                wc.Encoding = Encoding.UTF8;    //한글 깨짐 방지

                wc.Headers.Add("X-Naver-Client-Secret", "hhU6O3yWc1");  //Secret key
                wc.Headers.Add("X-Naver-Client-Id", "_f9vKg9IePJrUR1ts66E");  //Client ID
                LoadedXML = wc.DownloadString("https://openapi.naver.com/v1/search/encyc.xml?query=" + data + "&start=1&display=1");

                XmlReader reader = XmlReader.Create(new StringReader(LoadedXML));

                reader.ReadToFollowing("item");    //  item 이라는 이름을 가진 노드
                reader.ReadToFollowing("description");

                string xValue = reader.ReadElementContentAsString();    //descriptin이라는 노드의 값

                return xValue;
            }
            catch (Exception)
            {
                return "다시 입력해 주세요.";
            }
        }

        [WebMethod]
        public static string blog_Function(String data)     //네이버 검색 API를 통해 블로그 내용을 가져올 경우
        {
            try
            {
                String LoadedXML;

                WebClient wc = new WebClient();

                wc.Encoding = Encoding.UTF8;    //한글 깨짐 방지

                wc.Headers.Add("X-Naver-Client-Secret", "hhU6O3yWc1");
                wc.Headers.Add("X-Naver-Client-Id", "_f9vKg9IePJrUR1ts66E");
                LoadedXML = wc.DownloadString("https://openapi.naver.com/v1/search/blog.xml?query=" + data + "&start=1&display=1&sort=sim");

                XmlReader reader = XmlReader.Create(new StringReader(LoadedXML));

                reader.ReadToFollowing("item");    //  item이라는 이름을 가진 노드
                reader.ReadToFollowing("link");

                string xValue = reader.ReadElementContentAsString();    //link노드의 값

                return xValue;
            }
            catch (Exception)
            {
                return "다시 입력해 주세요."; //"http://www.google.co.kr/search?complete=1&hl=ko&lr=&aq=f&q=" + data; 
            }
        }

        [WebMethod]
        public static String GetContents(String txtmsg)  //파라미터 txtmsg: Client 단에서 넘어온 질문 값 
        {
            try
            {
                String responseText="";

                if (t.Eval(txtmsg).Equals(""))   //내부 DB에 값이 없으면 simsimiapi를 통해서 JSON 형태로 답을 받는다.
                {
                    if (txtmsg.IndexOf("뭐야") > -1)
                     {
                        var line1 = txtmsg.Split("뭐야");
                        if (line1[0].Trim() != "" && line1[0].Trim() != "가" && line1[0].Trim() != "이" && line1[0].Trim() != "는")
                        {
                          string[] line = new string[3];
                          if (txtmsg.IndexOf("가 뭐야") > -1) { line = txtmsg.Split("가 뭐야"); }
                          else if (txtmsg.IndexOf("이 뭐야") > -1) { line = txtmsg.Split("이 뭐야"); }
                          else if (txtmsg.IndexOf("는 뭐야") > -1) { line = txtmsg.Split("는 뭐야"); }
                          else { line = txtmsg.Split("뭐야"); }
                    
                          //<a href=" + result + " target=_blank >" + response + "</a><br><br>";
                          var line2 = def_Function(line[0]);
                          var line3 = line2.Split('.');
        
                          string responseText2 = line3[0] + '.';
                          if (responseText2.IndexOf("'") > -1)
                          {
                              responseText2 = responseText2.Replace("'", " ");
                          }
                         //responseText2 += "<br><a href=" + blog_Function(line[0]) + " target=_blank>" +line[0]+ " 관련 정보입니다.</a>";
                          if (responseText2 != "다시 입력해 주세요.")
                          {
                              t.AddConversation(line[0], responseText2);
                              t.SaveQuestion(line[0]);
                              t.SaveAnswer(responseText2);

                              var testData = new testData
                              {
                                  response = t.Eval(line[0])
                              };
                              return responseText = JsonConvert.SerializeObject(testData);
                          }
                        }
                     }

                        //Simsmi api를 통해서 답을 가져올 경우(Key=본인의 Key를 넣어야 합니다.)
                        WebClient wc = new WebClient();

                        wc.Encoding = Encoding.UTF8;    //한글 깨짐 방지
                        responseText = wc.DownloadString("http://api.simsimi.com/request.p?key=228fc7bf-8549-469d-a170-3fa768568212&lc=ko&ft=1.0&text=" + txtmsg);
                }

                else  //내부 DB에 값이 있으면
                {
                    var testData = new testData
                    {
                        response = t.Eval(txtmsg)
                    };
                    responseText = JsonConvert.SerializeObject(testData);
                }
                return responseText; //JSON 형태로 값 반환
            }
            catch (Exception)
            {
                var testData = new testData
                {
                    response = "다시 입력해주세요."
                };
                String responseText = JsonConvert.SerializeObject(testData);
                return responseText;
            }
        }

        public class testData
        {
            public string response;
        }
    }
}