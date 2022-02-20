using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 수동으로 하는 경우
namespace Server
{
    interface ITask
    {
        void Execute();
    }

    // class를 하나하나 직접 만들어줌
    // 이렇게 수동으로 하는 경우가 더 많은 것 같다.
    // lambda를 사용할 때 조심해야할 부분이 여러가지가 있다. - 직관적이지 않은 상황이 있음
    // 결론 : Task로 하면 lock을 이용해서하는것보다 개선할 수 있다.
    class Broadcast : ITask
    {
        GameRoom _room;
        ClientSession _session;
        string _chat;

        Broadcast(GameRoom room, ClientSession session, string chat)
        {
            _room = room;
            _session = session;
            _chat = chat;
        }

        public void Execute()
        {
            _room.Broadcast(_session, _chat);
        }
    }


    class TaskQueue
    {
        Queue<ITask> _queue = new Queue<ITask>();
        // jobqueue에 했던것처럼 어느시점에 flush하던 아니면 따로 task를 실행하는 애를 별도로 두거나 할 수 있다
    }
}

// 테스트할때는 다양한 시나리오를 준비해야한다.
// 패킷번호를 매겨서 5초뒤에 다시 나한테 정상적으로 패킷응답이 올것인지 
// 무한정으로 메모리가 늘어나는건 나쁜 신호 - thread pool이 돌아오지 않으면 새로운 thread를 뽑아와서 실행
// 