﻿/*
 * 사용자: philly
 * 날짜: 2016-06-11
 * 시간: 오전 1:09
 */
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using MySql.Data.MySqlClient;

namespace WebApplication1
{
    [Serializable]
    /// <summary>
    /// Corpus 는 말뭉치라는 의미로, 분리된 토큰들이 가르키는 문장들과 그에 대한 가중치를 포함한다.
    /// </summary>
    public struct Corpus
    {
        public Corpus(string sentence, int Rweight = 0)
        {
            this.Sentence = sentence;
            this.RWeight = Rweight;
        }

        public string Sentence;
        /// <summary>
        /// Rweight 는 가중치의 역수이다. 즉, 몇개의 토큰이 이 문장을 가르키는지를 의미한다.
        /// </summary>
        public int RWeight;

        /// <summary>
        /// 추가로 한개의 토큰이 이 문장을 가르치게 한다. 즉, 가중치의 역수에 1을 더해준다.
        /// </summary>
        public void PlusWeight()
        {
            RWeight++;
        }

        /// <summary>
        /// 각 말뭉치의 가중치는 말뭉치를 가르키는 토큰들의 개수를 N 이라 했을때, 1.0 / N 이 된다.
        /// </summary>
        /// <returns>가르키는 토큰의 수의 역수인 가중치를 리턴한다.</returns>
        public float Weight()
        {
            return 1.0f / RWeight;
        }
    }

    [Serializable]
    /// <summary>
    /// Corpus 와의 호환성을 위한 가중치 저장만을 위한 구조체.
    /// </summary>
    public struct OldCorpus
    {
        public OldCorpus(string sentence, float weight)
        {
            this.Sentence = sentence;
            this.Weight = weight;
        }

        public string Sentence;
        public float Weight;
    }

    [Serializable]
    public class Table
    {
        public Table()
        {
            table = new Dictionary<string, int[]>();
            corpuses = new List<Corpus>();
        }
        public Dictionary<string, int[]> table;
        public List<Corpus> corpuses;
    }

    public class CBR
    {
        Table _table;

        public Table Table { get { return _table; } }

        public CBR()
        {
            Init();
        }

        /// <summary>
        /// 테이블을 불러오고 각각의 가중치를 설정한다.
        /// </summary>
        public void Init()
        {
            _table = new Table();

            //원래 문장.
            //A: I want kitten
            //B: Can we put mittens on it?
            //A: I want food
            //B: Me too, I'm hungry
            //A: They had good food at restaurant
            //B: What kind did they have?

            /*
            _table = new Dictionary<string, int[]>()
            {
                {"i", new int[]{ 0, 1 } },
                {"want", new int[]{ 0, 1 } },
                {"kitten", new int[]{ 0 } },
                {"food", new int[]{ 1, 2 } },
                {"they", new int[]{ 2 } },
                {"had", new int[]{ 2 } },
                {"good", new int[]{ 2 } },
                {"at", new int[]{ 2 } },
                {"restaurant", new int[]{ 2 } },
            };
            _corpuses = new List<Corpus>()
            {
                {new Corpus("Can we put mittens on it?",  3)},
                {new Corpus("Me too, I'm hungry",  3)},
                {new Corpus("What kind did they have??",  6)}
            };
            */

            /*
            AddConversation("Hello", "Hi!");
            AddConversation("Hi", "Hi!");
            AddConversation("Are you happy?", "I'm sorry, but I am just a bot. What do you want?");
            AddConversation("I am hungry", "I am hungry, too.");
            AddConversation("I want food", "Me too, I'm hungry");
            AddConversation("What is your name", "My name is PhillyAI");
            AddConversation("What is your favorite food", "My favorite food is watermelon.");
            AddConversation("They had good food at restaurant", "What kind did they have?");
            AddConversation("What is 1 + 1", "Are you testing me? ;(");
            AddConversation("What do you think of me", "You are nice man, or woman. :)");
            AddConversation("Do you know", "Sure. Are you thinking me as fool? QAQ");
            */
        }

        //Mysql DB를 불러올 경우
        public void Loadlog()
        {
            // DB 연결
            string constr = "Server=localhost;Database=learningdata;Uid=learningdata;Pwd=1234;";

            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                conn.Open();
                string sql = "SELECT * FROM data";
                MySqlCommand cmd = new MySqlCommand(sql, conn); // query 명령문 날림
                MySqlDataReader rdr = cmd.ExecuteReader();  // Data 검색

                rdr.Read();  // DB로 부터 한 줄 읽어오기
                string _old = Parser.ParseKakaotalkLog((string)rdr[0]); // 맨 첫줄 _old에 저장
                string _new;

                while (rdr.Read())    // DB로부터 한줄 씩 읽어오기
                {
                    if ((_new = (string)rdr[0]) != null)  // _new에 _old 다음 줄 저장
                    {
                        _new = Parser.ParseKakaotalkLog(_new);  // 읽어온 줄을 파서에 전달
                        if (String.IsNullOrEmpty(_new) || String.IsNullOrWhiteSpace(_new)) { _old = _new; continue; }
                        if (string.IsNullOrEmpty(_old) || String.IsNullOrWhiteSpace(_old)) { _old = _new; continue; }
                        AddConversation(_old, _new);    // 읽어온 data 프로그램에 저장

                        _old = _new;    // 새로 저장 후 실행
                    }

                }

                rdr.Close();    // DB 접속 종료
            }

        }

        public void SaveQuestion(string Log)
        {
            string question = Log;
            string constr = "Server=localhost;Database=learningdata;Uid=learningdata;Pwd=1234;";
            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                conn.Open();
                MySqlCommand cmd0 = new MySqlCommand("INSERT INTO data VALUES (\'" + null + "\')", conn);
                MySqlCommand cmd1 = new MySqlCommand("INSERT INTO data VALUES (\'" + question+ "\')", conn);
                cmd0.ExecuteNonQuery();
                cmd1.ExecuteNonQuery();
            }

        }

        public void SaveAnswer(string Log)
        {
            string answer = Log;
            string constr = "Server=localhost;Database=learningdata;Uid=learningdata;Pwd=1234;";
            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                conn.Open();

                MySqlCommand cmd2 = new MySqlCommand("INSERT INTO data VALUES (\'" + answer + "\')", conn);

                cmd2.ExecuteNonQuery();

            }

        }


        //txt 파일을 불러올 경우
        //public void Loadlog()
        //{
        //    using (FileStream fs = new FileStream("C:/Users/latentbox/Documents/data.txt", FileMode.Open))
        //    {
        //        StreamReader streamreader = new StreamReader(fs);
        //        string formerString = Parser.ParseKakaotalkLog(streamreader.ReadLine());
        //        string latterString;

        //        while ((latterString = streamreader.ReadLine()) != null)
        //        {
        //            latterString = Parser.ParseKakaotalkLog(latterString);
        //            if (String.IsNullOrEmpty(latterString) || String.IsNullOrWhiteSpace(latterString)) { formerString = latterString; continue; }
        //            if (string.IsNullOrEmpty(formerString) || String.IsNullOrWhiteSpace(formerString)) { formerString = latterString; continue; }
        //            AddConversation(formerString, latterString);
        //            formerString = latterString;
        //        }
        //    }
        //}


        /// <summary>
        /// 새로운 대화를 학습시킨다.
        /// </summary>
        /// <param name="A">사용자가 말한 문장.</param>
        /// <param name="B">응답한 문장.</param>
        public void AddConversation(string A, string B)
        {
            string[] parsed_a = Parser.ParseKorean(A);

            if (parsed_a.Length == 0 || parsed_a.Length == 1 && parsed_a[0] == "") return;

            //먼저 말뭉치 테이블에 말뭉치 가중치를 업데이트 해준다.
            if (!containSentence(_table.corpuses, B))
                _table.corpuses.Add(new Corpus(B));

            //메인 테이블에 키값과 말뭉치를 연결시켜준다.
            foreach (string a in parsed_a)
            {
                if (containSentence(_table.table, a))
                {
                    List<int> newCps = new List<int>(_table.table[a]);
                    newCps.Add(GetSentenceIndex(_table.corpuses, B));

                    _table.table[a] = newCps.ToArray();
                }
                else
                {
                    List<int> newCps = new List<int>();
                    newCps.Add(GetSentenceIndex(_table.corpuses, B));

                    _table.table.Add(a, newCps.ToArray());
                }
                _table.corpuses[GetSentenceIndex(_table.corpuses, B)] = new Corpus(B, _table.corpuses[GetSentenceIndex(_table.corpuses, B)].RWeight + 1);
            }
        }
        /// <summary>
        /// 테이블을 파일로 저장
        /// </summary>
        /// <param name="path">파일 경로</param>
        public void SaveTable(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, _table);
            }
        }

        /// <summary>
        /// 테이블을 파일로부터 불러옴
        /// </summary>
        /// <param name="path">파일 경로</param>
        public void LoadTable(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                _table = (Table)bf.Deserialize(fs);
            }
        }

        public string Eval(string input)
        {
            string[] tokens = Parser.ParseKorean(input);
            List<string> sep = new List<string>();

            foreach (string s in tokens)
            {
                if (_table.table.ContainsKey(s))
                    sep.Add(s);
            }

            string[] separated = sep.ToArray();

            //토큰들의 하위 노드의 문장들의 가중치의 합을 구하기.
            float[] sum = new float[separated.Length];

            for (int i = 0; i < separated.Length; i++)
            {
                int[] sentences = _table.table[separated[i]];
                foreach (int index in sentences)
                {
                    sum[i] += _table.corpuses[index].Weight();
                }
            }

            //가중치를 새로 조정. (각 토큰들에 대하여 토큰의 문장 / 토큰의 하위 노드들의 문장들의 가중치 합의 합.)

            List<OldCorpus> newCps = new List<OldCorpus>();
            for (int i = 0; i < separated.Length; i++)
            {
                foreach (int index in _table.table[separated[i]])
                {
                    if (containSentence(newCps, _table.corpuses[index].Sentence))
                        newCps[GetSentenceIndex(newCps, _table.corpuses[index].Sentence)]
                            = new OldCorpus(_table.corpuses[index].Sentence, newCps[GetSentenceIndex(newCps, _table.corpuses[index].Sentence)].Weight + _table.corpuses[index].Weight() / sum[i]);
                    else
                        newCps.Add(new OldCorpus(_table.corpuses[index].Sentence, _table.corpuses[index].Weight() / sum[i]));
                }
            }

            //새로 조정된 가중치중 가장 큰 가중치를 가진 문장을 결과로 구한다.
            string result = ""; float maxWeight = 0;
            foreach (OldCorpus c in newCps)
            {
                if (c.Weight > maxWeight)
                {
                    maxWeight = c.Weight;
                    result = c.Sentence;
                }
            }

            //새로운 학습.
            //AddConversation(input, result);

            return result;
        }

        /// <summary>
        /// Corpus 리스트에서 문장을 포함 여부.
        /// </summary>
        /// <param name="cps">문장들 리스트</param>
        /// <param name="sentence">찾을 문장</param>
        /// <returns>문장을 포함하면 True, 포함하지 않으면 False 리턴.</returns>
        private bool containSentence(List<Corpus> cps, string sentence)
        {
            foreach (Corpus c in cps)
            {
                if (c.Sentence == sentence) return true;
            }
            return false;
        }
        private bool containSentence(List<OldCorpus> cps, string sentence)
        {
            foreach (OldCorpus c in cps)
            {
                if (c.Sentence == sentence) return true;
            }
            return false;
        }
        private bool containSentence(Dictionary<string, int[]> cps, string sentence)
        {
            foreach (string key in cps.Keys)
            {
                if (key == sentence) return true;
            }
            return false;
        }

        /// <summary>
        /// Corpus 리스트에서 포함하는 문장의 인덱스.
        /// </summary>
        /// <param name="cps">문장들 리스트</param>
        /// <param name="sentence">찾을 문장</param>
        /// <returns>리스트에서 문장에서의 인덱스. 만약 문장이 없다면 -1을 리턴.</returns>
        private int GetSentenceIndex(List<Corpus> cps, string sentence)
        {
            int index = 0;
            foreach (Corpus c in cps)
            {
                if (c.Sentence == sentence)
                    break;
                index++;
            }

            return index;
        }
        private int GetSentenceIndex(List<OldCorpus> cps, string sentence)
        {
            int index = 0;
            foreach (OldCorpus c in cps)
            {
                if (c.Sentence == sentence)
                    break;
                index++;
            }

            return index;
        }


    }
}