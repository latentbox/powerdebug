/*
 * 사용자: philly
 * 날짜: 2016-06-11
 * 시간: 오전 1:08
 */

 //#define PARSETEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WebApplication1
{
	static class Program
	{

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
		public static void Main(string[] args)
		{
#if PARSETEST
            while (true) {
                string[] str = Parser.ParseKorean(Console.ReadLine());
                foreach (string s in str)
                    Console.Write(s + ", ");
                Console.WriteLine(); }
# else 
           // Application.EnableVisualStyles();
           // Application.SetCompatibleTextRenderingDefault(false);
           // Application.Run(new Form2());

          
#endif
        }

        
    }
}