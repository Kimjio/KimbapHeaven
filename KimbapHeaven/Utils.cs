using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace KimbapHeaven
{
    public static class Utils
    {
        private static StorageFile xmlFile;

        private static readonly FoodData.Type[] types = { FoodData.Type.NEW, FoodData.Type.KIMBAP, FoodData.Type.MEAL, FoodData.Type.FLOURBASED, FoodData.Type.PORKCUTLET, FoodData.Type.SEASON };

        private static List<FoodData> FoodDatas = new List<FoodData>();
        private static List<FoodData> FoodDatasNEW = new List<FoodData>();
        private static List<FoodData> FoodDatasKIMBAP = new List<FoodData>();
        private static List<FoodData> FoodDatasMEAL = new List<FoodData>();
        private static List<FoodData> FoodDatasFLOURBASED = new List<FoodData>();
        private static List<FoodData> FoodDatasPORKCUTLET = new List<FoodData>();
        private static List<FoodData> FoodDatasSEASON = new List<FoodData>();

        private static List<Cate> Cates = new List<Cate>();

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

                foreach (FoodData.Type type in types)
                {
                    await ParseHTML(type);
                }

                progressListener?.Invoke(0, "데이터 저장 중...", true);
                WriteToXml(Cates);
            }

            progressListener?.Invoke(0, "메뉴 불러오는 중...", true);
            await LoadMenu(progressListener);
        }

        private async static Task ParseHTML(FoodData.Type type)
        {
            List<Food> Foods = new List<Food>();
            string url = "http://www.kimbab1009.com/" + (int) type;

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

                if (name == string.Empty)
                {
                    continue;
                }
                if (ContainsFoodByName(Foods, name))
                {
                    continue;
                }

                string imgUrl;

                if (type != FoodData.Type.SEASON)
                    imgUrl = ItemContainer.SelectSingleNode("//div[@class='img_wrap  ']").GetAttributeValue("data-src", string.Empty);
                else
                    imgUrl = ItemContainer.SelectSingleNode("//div[@class='img_wrap _lightbox_item cursor_pointer ']").GetAttributeValue("data-src", string.Empty);


                Foods.Add(new Food(name, imgUrl));
            }
            Cates.Add(new Cate(type, Foods));
        }

        private async static void WriteToXml(List<Cate> cates)
        {
            XmlWriter xmlWriter = XmlWriter.Create(await xmlFile.OpenStreamForWriteAsync());
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Menu");
            {
                foreach (Cate cate in cates)
                {
                    xmlWriter.WriteStartElement("Cate");
                    xmlWriter.WriteAttributeString("id", ((int) cate.type).ToString());
                    {
                        foreach (Food food in cate.foods)
                        {
                            xmlWriter.WriteStartElement("Food");
                            {
                                xmlWriter.WriteAttributeString("name", food.Name);
                                xmlWriter.WriteElementString("Price", "6500");
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
                FoodData.Type type = GetType(cateNode.Attributes["id"].Value);
                for (int indexFood = 0; indexFood < cateNode.SelectNodes("Food").Count; indexFood++)
                {
                    XmlNode foodNode = cateNode.SelectNodes("Food")[indexFood];
                    FoodDatas.Add(await new FoodData(type, foodNode.Attributes["name"].Value, int.Parse(foodNode.SelectSingleNode("Price").InnerText), 0, new Uri(foodNode.SelectSingleNode("Url").InnerText)).PreloadImage(indexCate, foodCount, menuNode.ChildNodes.Count, foodMax, progress));
                    foodCount++;
                }
            }

            BindLists();
        }

        public static List<FoodData> GetMenu()
        {
            return FoodDatas;
        }

        private static void BindLists()
        {
            FoodDatasNEW = GetMenuByType(FoodData.Type.NEW);
            FoodDatasKIMBAP = GetMenuByType(FoodData.Type.KIMBAP);
            FoodDatasMEAL = GetMenuByType(FoodData.Type.MEAL);
            FoodDatasFLOURBASED = GetMenuByType(FoodData.Type.FLOURBASED);
            FoodDatasPORKCUTLET = GetMenuByType(FoodData.Type.PORKCUTLET);
            FoodDatasSEASON = GetMenuByType(FoodData.Type.SEASON);
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
            }

            return foodDatas;
        }

        public static FoodData.Type GetType(string type)
        {
            switch (type)
            {
                case "31":
                    return FoodData.Type.NEW;
                case "25":
                    return FoodData.Type.KIMBAP;
                case "26":
                    return FoodData.Type.MEAL;
                case "27":
                    return FoodData.Type.FLOURBASED;
                case "28":
                    return FoodData.Type.PORKCUTLET;
                case "29":
                    return FoodData.Type.SEASON;
                default:
                    return FoodData.Type.UNDIFINED;
            }
        }

        public static List<FoodData> ConvertListToFoodDataList(List<object> target)
        {
            List<FoodData> foodDatas = new List<FoodData>();

            foreach (object item in target)
            {
                foodDatas.Add((FoodData) item);
            }

            return foodDatas;
        }

        private static bool ContainsFoodByName(List<Food> foods, string name)
        {
            foreach (Food food in foods)
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

        public static int GetAllFoodsCount(XmlNode menuNode)
        {
            int Count = 0;

            foreach (XmlNode cateNode in menuNode.SelectNodes("Cate"))
            {
                Count += cateNode.ChildNodes.Count;
            }

            return Count;
        }

        public static async Task<BitmapImage> ResizedImage(Uri imageSource, int maxWidth, int maxHeight, int indexCate, int indexFood, int cateMax, int foodMax, UpdateProgress progress)
        {
            BitmapImage sourceImage = new BitmapImage();
            sourceImage.ImageOpened += (object sender, RoutedEventArgs e) =>
            {
                double cateProgress = ((indexCate + 1) * 100d / cateMax);
                double foodProgress = ((indexFood + 1) * 100d / foodMax);
                int totalProgress = Convert.ToInt32(Math.Round((cateProgress + foodProgress) * 100 / 200));
                progress?.Invoke(totalProgress, "이미지 불러오는 중... " + cateMax + " / " + (indexCate + 1), false);
            };
            sourceImage.SetSource(await RandomAccessStreamReference.CreateFromUri(imageSource).OpenReadAsync());
            var origHeight = sourceImage.PixelHeight;
            var origWidth = sourceImage.PixelWidth;
            var ratioX = maxWidth / (float) origWidth;
            var ratioY = maxHeight / (float) origHeight;
            var ratio = Math.Min(ratioX, ratioY);
            var newHeight = (int) (origHeight * ratio);
            var newWidth = (int) (origWidth * ratio);

            sourceImage.DecodePixelWidth = newWidth;
            sourceImage.DecodePixelHeight = newHeight;

            return sourceImage;
        }

        private class Cate
        {
            public readonly FoodData.Type type;
            public readonly List<Food> foods;

            public Cate(FoodData.Type type, List<Food> foods)
            {
                this.type = type;
                this.foods = foods;
            }
        }

        private class Food
        {
            public readonly string Name;
            public readonly string ImgUrl;

            public Food(string Name, string ImgUrl)
            {
                this.Name = Name;
                this.ImgUrl = ImgUrl;
            }
        }
    }
}
