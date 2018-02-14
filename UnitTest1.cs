using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using Xunit;
using Xunit.Sdk;

namespace SimpleTests
{
    class ManyTests : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            for (var c = 0; c < 200; c++)
                yield return new object[] {c};
        }
    }
    
    public class UnitTest1 : UnitTestBase{}
    public class UnitTest2 : UnitTestBase{}
    public class UnitTest3 : UnitTestBase{}
    public class UnitTest4 : UnitTestBase{}
    public class UnitTest5 : UnitTestBase{}
    
    
    public class SeleniumTestBase : IDisposable
    {
        private static readonly object Lock = new  object();
        List<IDisposable> _disposables = new List<IDisposable>();
        
        protected T AddDisposable<T>(T d) where T : IDisposable
        {
            _disposables.Add(d);
            return d;
        }

        public void Dispose()
        {
            foreach (var d in _disposables.AsEnumerable().Reverse())
                d.Dispose();
        }

        private  RemoteWebDriver _driver;

        protected RemoteWebDriver Driver
        {
            get
            {
                if (_driver == null)
                {

                        var opts = new ChromeOptions();
                        if (!Debugger.IsAttached)
                        {
                            opts.AddArgument("headless");
                            opts.AddArgument("window-size=1920x1200");
                        }
                        _driver = AddDisposable(new ChromeDriver(opts));
                    
                    _driver.Manage().Window.Maximize();
                }
                return _driver;
            }
        }

        protected bool IsReady => Driver.ExecuteScript("return document.readyState").ToString() == "complete";

        protected void WaitForReady()
        {
            TimeoutAssert(() => IsReady);
        }
        
        public void TimeoutAssert(Func<bool> condition, TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(20);
            var started = DateTime.UtcNow;
            while (!condition())
            {
                if (!Debugger.IsAttached && DateTime.UtcNow > started + timeout.Value)
                    throw new TimeoutException();
                Thread.Sleep(200);
            }
        }
    }
    
    public class UnitTestBase : SeleniumTestBase
    {

        [Theory, ManyTests]
        public void Test1(int pass)
        {
            Driver.Navigate().GoToUrl("http://ya.ru/");
            WaitForReady();
            TimeoutAssert(() => false, new TimeSpan(60));
        }



        
        
    }
}
