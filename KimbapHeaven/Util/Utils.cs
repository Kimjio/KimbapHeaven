using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace KimbapHeaven
{
    public static class Utils
    {
        public const string KOREA_EAN_CODE = "880";

        public const int DEFAULT_SEAT_SIZE = 10;
        private const int FALLBACK_PRICE = 3400;

        private static StorageFile xmlFile;

        public static readonly FoodData.Type[] FoodTypes = { FoodData.Type.NEW, FoodData.Type.KIMBAP, FoodData.Type.MEAL, FoodData.Type.FLOURBASED, FoodData.Type.PORKCUTLET, FoodData.Type.SEASON, FoodData.Type.UNDIFINED };
        private static List<FoodData> FoodDatas = new List<FoodData>();
        private static List<FoodData> FoodDatasNEW = new List<FoodData>();
        private static List<FoodData> FoodDatasKIMBAP = new List<FoodData>();
        private static List<FoodData> FoodDatasMEAL = new List<FoodData>();
        private static List<FoodData> FoodDatasFLOURBASED = new List<FoodData>();
        private static List<FoodData> FoodDatasPORKCUTLET = new List<FoodData>();
        private static List<FoodData> FoodDatasSEASON = new List<FoodData>();
        private static List<FoodData> FoodDatasUNDIFINED = new List<FoodData>();

        private static List<XmlCate> Cates = new List<XmlCate>();

        public enum ImageQuality
        {
            High = 0,
            Middle = 1,
            Low = 2
        }

        /// <summary>
        /// 백분율이 변경되었을 때 처리할 메소드를 나타냅니다.
        /// </summary>
        /// <param name="progress">변경된 백분율</param><param name="message">현재 상태</param><param name="indeterminate">확정 여부</param>
        public delegate void UpdateProgress(int progress, string message, bool indeterminate);

        public async static Task GetMenuAsync(UpdateProgress progressListener)
        {
            bool Exists = await LoadFile();
            progressListener?.Invoke(0, "파일 불러오는 중...", true);

            if (!Exists)
            {
                progressListener?.Invoke(0, "데이터 가져오는 중...", true);

                foreach (FoodData.Type type in FoodTypes)
                {
                    if (type == FoodData.Type.UNDIFINED) continue;
                    await ParseHTML(type);
                }

                progressListener?.Invoke(0, "데이터 저장 중...", true);
                WriteToXml(Cates);
            }

            progressListener?.Invoke(0, "메뉴 불러오는 중...", true);
            await LoadMenu(progressListener);
        }

        public static ImageQuality GetCurrentImageQuality()
        {
            ImageQuality imageQuality = ImageQuality.High;
            string QualityString = Settings.GetString("ImageQuality", "High");
            switch (QualityString)
            {
                case "High":
                    imageQuality = ImageQuality.High;
                    break;

                case "Middle":
                    imageQuality = ImageQuality.Middle;
                    break;

                case "Low":
                    imageQuality = ImageQuality.Low;
                    break;
            }

            return imageQuality;
        }

        public static FoodData FindFoodDataByEANCode(string receivedBarcodeChars)
        {
            foreach (FoodData food in FoodDatas)
            {
                if (food.EANCode.Equals(receivedBarcodeChars))
                    return food;
            }

            return null;
        }

        public static FoodData FindFoodDataByName(IList<object> foods, string name)
        {
            foreach (FoodData food in foods)
            {
                if (food.Name.Equals(name))
                    return food;
            }

            return null;
        }

        public static FoodData FindFoodDataByName(IList<FoodData> foods, string name)
        {
            foreach (FoodData food in foods)
            {
                if (food.Name.Equals(name))
                    return food;
            }

            return null;
        }

        private async static Task ParseHTML(FoodData.Type type)
        {
            List<XmlFood> Foods = new List<XmlFood>();
            string url = "http://www.kimbab1009.com/" + (int)type;

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = await web.LoadFromWebAsync(url);

            HtmlNode docNodes = doc.DocumentNode;

            HtmlNodeCollection ItemGallarys = docNodes.SelectSingleNode("//main").SelectSingleNode("//div[@class='_widget_data']").SelectNodes("//div[@class='_item item_gallary ']");
            foreach (HtmlNode ItemGallary in ItemGallarys)
            {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(ItemGallary.InnerHtml);
                HtmlNode ItemContainer = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='item_container _item_container']");
                HtmlNode title = ItemContainer.SelectSingleNode("//p[@class='title']");
                if (title.SelectSingleNode("//span") != null)
                    title.RemoveChild(title.SelectSingleNode("//span"));
                string name = ItemContainer.SelectSingleNode("//p[@class='title']").InnerText;

                if (name == string.Empty || ContainsFoodByName(Foods, name))
                {
                    continue;
                }

                string imgUrl;

                if (type != FoodData.Type.SEASON)
                    imgUrl = ItemContainer.SelectSingleNode("//div[@class='img_wrap  ']").GetAttributeValue("data-src", string.Empty);
                else
                    imgUrl = ItemContainer.SelectSingleNode("//div[@class='img_wrap _lightbox_item cursor_pointer ']").GetAttributeValue("data-src", string.Empty);


                Foods.Add(new XmlFood(name, imgUrl));
            }
            Cates.Add(new XmlCate(type, Foods));
        }

        private async static void WriteToXml(List<XmlCate> cates)
        {
            XmlWriter xmlWriter = XmlWriter.Create(await xmlFile.OpenStreamForWriteAsync());
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Menu");
            {
                foreach (XmlCate cate in cates)
                {
                    xmlWriter.WriteStartElement("Cate");
                    xmlWriter.WriteAttributeString("id", ((int)cate.type).ToString());
                    {
                        foreach (XmlFood food in cate.foods)
                        {
                            xmlWriter.WriteStartElement("Food");
                            {
                                xmlWriter.WriteAttributeString("name", food.Name);
                                xmlWriter.WriteElementString("Price", FALLBACK_PRICE.ToString());
                                xmlWriter.WriteElementString("Url", food.ImgUrl);
                            }
                            xmlWriter.WriteEndElement();
                        }
                    }
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        /// <summary>
        /// Menu.xml을 만들거나 불러옵니다.
        /// </summary>
        /// <returns>true: 파일이 존재 함, false: 파일이 생성 됨</returns>
        private static async Task<bool> LoadFile()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            Debug.WriteLine(storageFolder.Path);
            try
            {
                xmlFile = await storageFolder.CreateFileAsync("Menu.xml", CreationCollisionOption.FailIfExists);
                return false;
            }
            catch (Exception)
            {
                xmlFile = await storageFolder.GetFileAsync("Menu.xml");
                return true;
            }
        }

        public static async void ClearFile()
        {
            await xmlFile.DeleteAsync();
        }

        private async static Task LoadMenu(UpdateProgress progress)
        {
            var input = await xmlFile.OpenReadAsync();

            XmlTextReader xmlTextReader = new XmlTextReader(input.AsStreamForRead());
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlTextReader);
            XmlNode menuNode = xmlDocument.SelectSingleNode("Menu");
            int foodMax = GetAllFoodsCount(menuNode);
            int foodCount = 0;

            for (int indexCate = 0; indexCate < menuNode.SelectNodes("Cate").Count; indexCate++)
            {
                XmlNode cateNode = menuNode.SelectNodes("Cate")[indexCate];
                FoodData.Type type = FoodData.ParseType(cateNode.Attributes["id"].Value);
                for (int indexFood = 0; indexFood < cateNode.SelectNodes("Food").Count; indexFood++)
                {
                    XmlNode foodNode = cateNode.SelectNodes("Food")[indexFood];
                    if (foodNode.SelectSingleNode("EAN") == null)
                        FoodDatas.Add(await new FoodData(type, foodNode.Attributes["name"].Value, int.Parse(foodNode.SelectSingleNode("Price").InnerText), 0, new Uri(foodNode.SelectSingleNode("Url").InnerText)).PreloadImage(indexCate, foodCount, menuNode.ChildNodes.Count, foodMax, progress));
                    else
                        FoodDatas.Add(await new FoodData(type, foodNode.Attributes["name"].Value, foodNode.SelectSingleNode("EAN").InnerText, int.Parse(foodNode.SelectSingleNode("Price").InnerText), 0, new Uri(foodNode.SelectSingleNode("Url").InnerText)).PreloadImage(indexCate, foodCount, menuNode.ChildNodes.Count, foodMax, progress));

                    foodCount++;
                }
            }

            InitLists();
        }

        public async static Task<bool> AddCustomMenu(StorageFile path)
        {
            try
            {
                XDocument document = XDocument.Load(await xmlFile.OpenStreamForReadAsync());
                IEnumerable<XElement> xmlCates = document.Element("Menu").Elements("Cate");
                XElement cateNew = xmlCates.ElementAt(0);
                XElement cateKimbap = xmlCates.ElementAt(1);
                XElement cateMeal = xmlCates.ElementAt(2);
                XElement cateFlourbased = xmlCates.ElementAt(3);
                XElement catePorkcutlet = xmlCates.ElementAt(4);
                XElement cateSeason = xmlCates.ElementAt(5);
                XElement cateUndifined;
                try
                {
                    cateUndifined = xmlCates.ElementAt(6);
                }
                catch (ArgumentOutOfRangeException)
                {
                    XElement cate6 = new XElement("Cate", new XAttribute("id", (int)FoodData.Type.UNDIFINED));
                    document.Element("Menu").Add(cate6);
                    cateUndifined = cate6;
                }
                XDocument userDocument = XDocument.Load(await path.OpenStreamForReadAsync());
                IEnumerable<XElement> cates = userDocument.Element("Menu").Elements("Cate");

                foreach (XElement cate in cates)
                {
                    switch (int.Parse(cate.Attribute("id").Value))
                    {
                        case (int)FoodData.Type.NEW:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                cateNew.Add(food);
                            }
                            break;
                        case (int)FoodData.Type.KIMBAP:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                cateKimbap.Add(food);
                            }
                            break;
                        case (int)FoodData.Type.MEAL:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                cateMeal.Add(food);
                            }
                            break;
                        case (int)FoodData.Type.FLOURBASED:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                cateFlourbased.Add(food);
                            }
                            break;
                        case (int)FoodData.Type.PORKCUTLET:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                catePorkcutlet.Add(food);
                            }
                            break;
                        case (int)FoodData.Type.SEASON:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                cateSeason.Add(food);
                            }
                            break;
                        case (int)FoodData.Type.UNDIFINED:
                            foreach (XElement food in cate.Elements("Food"))
                            {
                                cateUndifined.Add(food);
                            }
                            break;
                    }
                }
                document.Save(await xmlFile.OpenStreamForWriteAsync());
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static List<FoodData> GetMenu()
        {
            return FoodDatas;
        }

        private static void InitLists()
        {
            FoodDatasNEW = GetMenuByType(FoodData.Type.NEW);
            FoodDatasKIMBAP = GetMenuByType(FoodData.Type.KIMBAP);
            FoodDatasMEAL = GetMenuByType(FoodData.Type.MEAL);
            FoodDatasFLOURBASED = GetMenuByType(FoodData.Type.FLOURBASED);
            FoodDatasPORKCUTLET = GetMenuByType(FoodData.Type.PORKCUTLET);
            FoodDatasSEASON = GetMenuByType(FoodData.Type.SEASON);
            FoodDatasUNDIFINED = GetMenuByType(FoodData.Type.UNDIFINED);
        }

        private static List<FoodData> GetMenuByType(FoodData.Type type)
        {
            List<FoodData> filteredFoodDatas = new List<FoodData>();
            foreach (FoodData foodData in FoodDatas)
                if (foodData.FoodType == type)
                    filteredFoodDatas.Add(foodData);
            return filteredFoodDatas;
        }

        public static List<FoodData> GetMenu(FoodData.Type type)
        {
            List<FoodData> foodDatas = null;

            switch (type)
            {
                case FoodData.Type.NEW:
                    foodDatas = FoodDatasNEW;
                    break;
                case FoodData.Type.KIMBAP:
                    foodDatas = FoodDatasKIMBAP;
                    break;
                case FoodData.Type.MEAL:
                    foodDatas = FoodDatasMEAL;
                    break;
                case FoodData.Type.FLOURBASED:
                    foodDatas = FoodDatasFLOURBASED;
                    break;
                case FoodData.Type.PORKCUTLET:
                    foodDatas = FoodDatasPORKCUTLET;
                    break;
                case FoodData.Type.SEASON:
                    foodDatas = FoodDatasSEASON;
                    break;
                case FoodData.Type.UNDIFINED:
                    foodDatas = FoodDatasUNDIFINED;
                    break;
            }

            return foodDatas;
        }

        

        public static List<FoodData> ConvertListToFoodDataList(List<object> target)
        {
            List<FoodData> foodDatas = new List<FoodData>();

            foreach (object item in target)
            {
                foodDatas.Add((FoodData)item);
            }

            return foodDatas;
        }

        private static bool ContainsFoodByName(List<XmlFood> foods, string name)
        {
            foreach (XmlFood food in foods)
            {
                if (food.Name.Equals(name))
                    return true;
            }
            return false;
        }

        public static bool ContainsFoodDataByName(IList<object> foods, string name)
        {
            foreach (FoodData food in foods)
            {
                if (food.Name.Equals(name))
                    return true;
            }
            return false;
        }

        public static bool ContainsFoodDataByName(IList<FoodData> foods, string name)
        {
            foreach (FoodData food in foods)
            {
                if (food.Name.Equals(name))
                    return true;
            }
            return false;
        }

        public static int GetAllFoodsCount(XmlNode menuNode)
        {
            int Count = 0;

            foreach (XmlNode cateNode in menuNode.SelectNodes("Cate"))
            {
                Count += cateNode.ChildNodes.Count;
            }

            return Count;
        }

        public static async Task<BitmapImage> ResizedImage(Uri imageSource, int indexCate, int indexFood, int cateMax, int foodMax, UpdateProgress progress)
        {
            int max = 512;

            switch (GetCurrentImageQuality())
            {
                case ImageQuality.High:
                    max = 512;
                    break;

                case ImageQuality.Middle:
                    max = 256;
                    break;

                case ImageQuality.Low:
                    max = 128;
                    break;

            }

            BitmapImage sourceImage = new BitmapImage();
            sourceImage.ImageOpened += (object sender, RoutedEventArgs e) =>
            {
                double cateProgress = ((indexCate + 1) * 100d / cateMax);
                double foodProgress = ((indexFood + 1) * 100d / foodMax);
                int totalProgress = Convert.ToInt32(Math.Round((cateProgress + foodProgress) * 100 / 200));
                progress?.Invoke(totalProgress, string.Format("이미지 불러오는 중... {0} / {1}", cateMax, indexCate + 1), false);
            };
            sourceImage.SetSource(await RandomAccessStreamReference.CreateFromUri(imageSource).OpenReadAsync());
            var origHeight = sourceImage.PixelHeight;
            var origWidth = sourceImage.PixelWidth;
            var ratioX = max / (float)origWidth;
            var ratioY = max / (float)origHeight;
            var ratio = Math.Min(ratioX, ratioY);
            var newHeight = (int)(origHeight * ratio);
            var newWidth = (int)(origWidth * ratio);

            sourceImage.DecodePixelWidth = newWidth;
            sourceImage.DecodePixelHeight = newHeight;

            return sourceImage;
        }

        private class XmlCate
        {
            public readonly FoodData.Type type;
            public readonly List<XmlFood> foods;

            public XmlCate(FoodData.Type type, List<XmlFood> foods)
            {
                this.type = type;
                this.foods = foods;
            }
        }

        private class XmlFood
        {
            public readonly string Name;
            public readonly string ImgUrl;

            public XmlFood(string Name, string ImgUrl)
            {
                this.Name = Name;
                this.ImgUrl = ImgUrl;
            }
        }
    }
}
