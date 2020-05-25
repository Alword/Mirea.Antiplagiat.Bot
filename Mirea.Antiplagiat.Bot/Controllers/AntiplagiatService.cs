using Microsoft.Extensions.Logging;
using Mirea.Antiplagiat.Bot.Controllers;
using Mirea.Antiplagiat.Bot.Extentions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mirea.Antiplagiat.Bot.Models
{
    public class AntiplagiatService : IAntiplagiatService
    {
        public delegate void DocumentCheckedEvent(object sender, string documentPath, string resultPath);
        public event DocumentCheckedEvent OnDocumentChecked;

        private readonly Credentials credentials;
        private IWebDriver driver;
        private readonly ILogger<AntiplagiatService> logger;

        private int maxIdChecking = 0;
        private readonly Queue<string> documentsQueue;
        private readonly List<string> checkeding;

        public AntiplagiatService(Credentials credentials, ILogger<AntiplagiatService> logger)
        {
            this.logger = logger;
            this.credentials = credentials;
            this.documentsQueue = new Queue<string>();
            this.checkeding = new List<string>();

        }

        private IWebDriver InitDriwer()
        {
            if (driver != null)
            {
                driver.Close();
                driver.Dispose();
            }

            ChromeOptions options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", Folders.Repots());
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("disable-popup-blocking", "true");
            if (credentials.Hidden)
                options.AddArgument("headless");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            return new ChromeDriver(service, options);
        }

        public void Run(CancellationToken token)
        {
            driver = InitDriwer();
            logger.LogInformation(AppData.Strings.AuthorizeAntiplagiat);
            driver.Url = $"{credentials.Base}/cabinet";
            driver.Navigate();

            IWebElement loginButton = driver.FindElements(By.ClassName("enter")).FirstOrDefault();
            if (loginButton != null)
                loginButton = loginButton.FindElements(By.TagName("a")).FirstOrDefault();
            if (loginButton != null)
            {
                loginButton.Click();
                IWebElement emailTextbox = driver.FindElement(By.ClassName("email")).FindElement(By.TagName("input"));
                IWebElement passwordTextBox = driver.FindElement(By.ClassName("passwd")).FindElement(By.TagName("input"));
                IWebElement enterButton = driver.FindElement(By.Id("login-button"));

                emailTextbox.SendKeys(credentials.Login);
                passwordTextBox.SendKeys(credentials.Password);
                enterButton.Click();
            }

            logger.LogInformation(AppData.Strings.AuthorizeAntiplagiatSuccess);

            while (!token.IsCancellationRequested)
            {
                logger.LogInformation($"{AppData.Strings.Checking} ({documentsQueue.Count}) ({checkeding.Count})");

                if (documentsQueue.Any() && maxIdChecking < 10)
                {
                    string path = documentsQueue.Peek();
                    logger.LogInformation($"{AppData.Strings.Upload} {path}");
                    UploadDocument(path);
                    documentsQueue.Dequeue();
                }
                if (checkeding.Any())
                {
                    CheckStatus();
                }
                else
                {
                    maxIdChecking = 0;
                }
                Task.Delay(1000).Wait();
            }

        }

        private void CheckStatus()
        {
            maxIdChecking = 0;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("scroll-area")));
            IWebElement scrollPanel = driver.FindElement(By.ClassName("scroll-area"));

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(ExpectedConditions.ElementIsVisible(By.TagName("tr")));
            var webElements = scrollPanel.FindElements(By.TagName("tr"));
            for (int i = webElements.Count - 1; i >= 0; i--)
            {
                var data = webElements[i].FindElements(By.TagName("td"));
                string text = $"{data[1].Text} $ $".Split(' ')[1];
                var isDocumentInQueue = checkeding.Where(d => d.Contains(text)).FirstOrDefault();
                if (isDocumentInQueue != null)
                {
                    maxIdChecking = Math.Max(maxIdChecking, i);
                    IWebElement IsDucumentChecked = null;
                    try
                    {
                        IsDucumentChecked = webElements[i].FindElements(By.ClassName("report-link")).FirstOrDefault();
                    }
                    catch (StaleElementReferenceException ex)
                    {
                        continue;
                    }
                    if (IsDucumentChecked != null)
                    {
                        logger.LogInformation(AppData.Strings.Reply);
                        string reportPath = DownloadReport(webElements[i]);
                        checkeding.Remove(isDocumentInQueue);
                        OnDocumentChecked?.Invoke(this, isDocumentInQueue, reportPath);
                        driver.Navigate().GoToUrl("https://users.antiplagiat.ru/cabinet");
                        break;
                    }
                }
            }
        }


        private string DownloadReport(IWebElement checkResult)
        {
            checkResult.FindElement(By.ClassName("report-link")).Click();

            IWebElement exportSpoiler = driver.FindElement(By.XPath("/html/body/div[1]/main/div/div[1]/div[1]/div[2]/div[2]/div[2]/div[1]"));
            exportSpoiler.Click();
            IWebElement exportNavigation = driver.FindElement(By.Id("report-export"));
            exportNavigation.Click();

            while (driver.WindowHandles.Count < 2)
                Task.Delay(250).Wait();

            driver.SwitchTo().Window(driver.WindowHandles[1]);

            IWebElement export = driver.FindElements(By.ClassName("export-make")).FirstOrDefault();
            if (export != null)
                export.Click();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(600));
            logger.LogInformation(AppData.Strings.Exporting);
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("export-download")));
            IWebElement downloadButton = driver.FindElement(By.ClassName("export-download"));

            DirectoryInfo d = new DirectoryInfo(Folders.Repots());

            DateTime lastData = DateTime.Now;
            if (d.GetFiles().Any())
            {
                lastData = d.GetFiles().Max(d => d.LastWriteTime);
            }

            downloadButton.Click();

            FileInfo newFile = null;
            do
            {
                logger.LogInformation(AppData.Strings.Downloading);
                Task.Delay(5000).Wait();
                var newFiles = d.GetFiles().OrderByDescending(d => d.LastWriteTime);

                if (newFiles.Any())
                    newFile = newFiles.First();
            }
            while (newFile == null ||
            newFile.LastAccessTime < lastData && !newFile.FullName.EndsWith("crdownload"));

            driver.SwitchTo().Window(driver.WindowHandles[1]).Close();
            driver.SwitchTo().Window(driver.WindowHandles[0]);
            logger.LogInformation(AppData.Strings.Downloaded);
            return newFile.FullName;
        }

        private void UploadDocument(string path)
        {
            checkeding.Add(path);
            IWebElement fileUpload = driver.FindElement(By.Id("fileupload"));
            fileUpload.SendKeys(path);

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("file-upload-btn")));

            IWebElement continueButton = driver.FindElement(By.Id("file-upload-btn"));
            continueButton.Click();

            Task.Delay(1000).Wait();

            driver.Navigate().Refresh();
        }

        public void EnqueueDocument(string path)
        {
            documentsQueue.Enqueue(path);
        }
    }
}
