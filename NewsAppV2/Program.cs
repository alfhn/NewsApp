using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace NewsAppV2
{
    public class NewsItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string PubDate { get; set; }
    }

    public class MainForm : Form
    {
        private List<NewsItem> currentNews = new List<NewsItem>();
        private ListView listView;
        private ComboBox cmbSources;

        public MainForm()
        {
            this.Text = "NewsApp v2.0 - Влад";
            this.Size = new System.Drawing.Size(900, 600);
            SetupUI();
        }

        private void SetupUI()
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            var lbl = new Label { Text = "Источник:", Left = 10, Top = 10, Width = 60 };
            cmbSources = new ComboBox { Left = 70, Top = 7, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSources.Items.Add("Lenta.ru");
            cmbSources.Items.Add("Habr");
            cmbSources.SelectedIndex = 0;

            var btnLoad = new Button { Text = "Загрузить", Left = 280, Top = 5, Width = 100 };
            btnLoad.Click += async (s, e) => await LoadNewsAsync();

            topPanel.Controls.Add(lbl);
            topPanel.Controls.Add(cmbSources);
            topPanel.Controls.Add(btnLoad);

            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true
            };
            listView.Columns.Add("Заголовок", 500);
            listView.Columns.Add("Дата", 150);

            listView.DoubleClick += (s, e) => ShowNewsDetails();

            this.Controls.Add(listView);
            this.Controls.Add(topPanel);
        }

        private async Task LoadNewsAsync()
        {
            try
            {
                string url = cmbSources.SelectedIndex == 0
                    ? "https://lenta.ru/rss/news"
                    : "https://habr.com/ru/rss/news/";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "NewsApp/2.0");

                var rss = await client.GetStringAsync(url);
                currentNews = ParseRss(rss);

                listView.Items.Clear();
                foreach (var news in currentNews)
                {
                    var item = new ListViewItem(news.Title);
                    item.SubItems.Add(news.PubDate);
                    item.Tag = news;
                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private void ShowNewsDetails()
        {
            if (listView.SelectedItems.Count == 0) return;
            var news = listView.SelectedItems[0].Tag as NewsItem;

            var detailsForm = new Form
            {
                Text = news.Title,
                Size = new System.Drawing.Size(700, 500),
                StartPosition = FormStartPosition.CenterParent
            };

            var browser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                DocumentText = "<html><body style='font-family: Arial; padding: 20px;'>" +
                    "<h1>" + news.Title + "</h1>" +
                    "<p><b>Дата:</b> " + news.PubDate + "</p>" +
                    "<p>" + news.Description + "</p>" +
                    "<hr><p><a href='" + news.Link + "'>Открыть оригинал</a></p>" +
                    "</body></html>"
            };

            detailsForm.Controls.Add(browser);
            detailsForm.ShowDialog(this);
        }

        private List<NewsItem> ParseRss(string rssContent)
        {
            var news = new List<NewsItem>();
            var doc = XDocument.Parse(rssContent);
            foreach (var item in doc.Descendants("item"))
            {
                news.Add(new NewsItem
                {
                    Title = item.Element("title")?.Value ?? "Без заголовка",
                    Description = item.Element("description")?.Value ?? "",
                    Link = item.Element("link")?.Value ?? "",
                    PubDate = item.Element("pubDate")?.Value ?? ""
                });
            }
            return news;
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}