using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Controllers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mirea.Antiplagiat.Bot.Models
{
    public class AntiplagiatService : IAntiplagiatService
    {
        public delegate void DocumentCheckedEvent(object sender, string documentPath, string resultPath);
        public event DocumentCheckedEvent OnDocumentChecked;

        private readonly Credentials credentials;
        private readonly IWebDriver driver;
        private readonly ILogger<AntiplagiatService> logger;
        public AntiplagiatService(ILogger<AntiplagiatService> logger, Credentials credentials)
        {
            this.logger = logger;
            this.credentials = credentials;
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            driver = new ChromeDriver(service, options);
            Login();
        }

        private void Login()
        {
            logger.LogInformation(AppData.Strings.AuthorizeAntiplagiat);

            driver.Url = "https://users.antiplagiat.ru/cabinet";
            driver.Navigate();
            IWebElement loginButton = driver.FindElement(By.ClassName("enter")).FindElements(By.TagName("a")).FirstOrDefault();
            if (loginButton != null)
            {
                IWebElement emailTextbox = driver.FindElement(By.ClassName("email")).FindElement(By.TagName("input"));
                IWebElement passwordTextBox = driver.FindElement(By.ClassName("passwd")).FindElement(By.TagName("input"));
                IWebElement enterButton = driver.FindElement(By.Id("login-button"));

                emailTextbox.SendKeys(credentials.Login);
                passwordTextBox.SendKeys(credentials.Password);
                enterButton.Click();
            }
            logger.LogInformation(AppData.Strings.AuthorizeAntiplagiatSuccess);
        }

        public void EnqueueDocument(string path)
        {

        }
    }
}
