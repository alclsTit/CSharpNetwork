using System;

namespace ProjectWaterMelon.Log
{
    /// <summary>
    /// Console 창에 찍히는 로그 관련 클래스
    /// LogFactory 디자인 패턴에의해 생성되는 실제 객체
    /// 1. ILog.IsXXXEnabled 체크하는 것이 크진 않더라도 성능상 유리 
    /// 2. Exception 객체가 null 일 경우 object 타입만 파라미터로 받는 메서드로 처리. 별도의 익셉션 없음
    /// </summary>
    public sealed class CConsoleLog : CLogger
    {
        public CConsoleLog(string name, string nameOfConsoleTitle = "") :  base(name)
        {
            if (!string.IsNullOrEmpty(nameOfConsoleTitle))
                Console.Title = nameOfConsoleTitle;
        }

        public override void Debug(object message)
        {
            if (base.IsDebugEnabled)
                base.Debug(message);
        }

        public override void Debug(object message, Exception exception)
        {
            if (base.IsDebugEnabled)
                base.Debug(message, exception);
        }

        public override void Info(object message)
        {
            if (base.IsInfoEnabled)
                base.Info(message);
        }

        public override void Info(object message, Exception exception)
        {
            if (base.IsInfoEnabled)
                base.Info(message, exception);
        }

        public override void Warn(object message)
        {
            if (base.IsWarnEnabled)
                base.Warn(message);
        }

        public override void Warn(object message, Exception exception)
        {
            if (base.IsWarnEnabled)
                base.Warn(message, exception);
        }

        public override void Error(object message)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;

            if (base.IsErrorEnabled)
                base.Error(message);

            Console.ResetColor();
        }

        public override void Error(object message, Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;

            if (base.IsErrorEnabled)
                base.Error(message, exception);

            Console.ResetColor();
        }

        public override void Fatal(object message)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            if (base.IsFatalEnabled)
                base.Fatal(message);

            Console.ResetColor();
        }

        public override void Fatal(object message, Exception exception)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            if (base.IsFatalEnabled)
                base.Fatal(message, exception);

            Console.ResetColor();
        }
    }

    /// <summary>
    /// LogFactory 객체 생성 구현 파생클래스 
    /// </summary>
    public class CConsoleLogFactory : ILogFactory
    {
        public CLogger GetLogger()
        {
            return new CConsoleLog(nameof(CConsoleLog), "ServerModule");
        }
    }
}
