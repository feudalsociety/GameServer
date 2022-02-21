using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;


namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());
            JobTimer.Instance.Push(FlushRoom, 250); // 예약
        }

        static void Main(string[] args)
        {
            // PacketManager.Instance.Register();

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 개선을 하자면 중간에 SessionManager를 둬서 만들어주도록한다. session id나 count 관리
            // Manager는 core에 들어가도되고 컨텐츠단에서 관리해도 됨
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            // 시간 관리 tick을 이용
            // 패킷 핸들러에서는 queue에만 넣고 빠지고 실제 연산은 main에서 처리

            // int roomTick = 0;
            //
            //
            //while (true)
            //{
            //    // int now = System.Environment.TickCount;
            //    // if(roomTick < now)
            //    {
            //        Room.Push(() => Room.Flush());
            //        // roomTick = now + 250000;
            //        // 한참 있다가 실행되는 경우 불필요하게 check
            //        // 예약 system이 있으면 좋곘다. - unity에서 코루틴 이용
            //        // 컨텐츠 단 뿐만아니라 서버 쪽에도 그런거 있으면 편리할 것이다.
            //        // 구현방법은 여러가지 - 우선순위 큐 이용 
            //    }

            //    //
            //}

            //FlushRoom(); // 예약
            JobTimer.Instance.Push(FlushRoom);
            // 다른 thread에서 JobTimer라는 중앙관리 시스템에 일감을 던져서 예약하게될것

            while(true)
            {
                // 예약된거 처리
                JobTimer.Instance.Flush();
            }
        }
    }
}