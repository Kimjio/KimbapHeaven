using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KimbapHeaven
{
    public static class StateManager
    {
        private static readonly int[] TypesTotalPrice = { 0, 0, 0, 0, 0, 0, 0 };
        private static readonly int[] PayTypeCount = { 0, 0 };
        private static readonly int AllTotalPrice;

        private static readonly List<FoodData> FoodsNew = new List<FoodData>();
        private static readonly List<FoodData> FoodsKimbap = new List<FoodData>();
        private static readonly List<FoodData> FoodsMeal = new List<FoodData>();
        private static readonly List<FoodData> FoodsFlourbased = new List<FoodData>();
        private static readonly List<FoodData> FoodsPorkcutlet = new List<FoodData>();
        private static readonly List<FoodData> FoodsSeason = new List<FoodData>();
        private static readonly List<FoodData> FoodsUndified = new List<FoodData>();

        public enum PayType
        {
            Card = 0,
            Cash = 1
        };

        static StateManager()
        {
            AllTotalPrice = Settings.GetInt("allTotalPrice", 0);
        }

        public static void AddState(List<FoodData> foodDatas, PayType payType)
        {
            foreach (FoodData food in foodDatas)
            {
                switch (food.FoodType)
                {
                    case FoodData.Type.NEW:
                        AddIfEmpty(FoodsNew, food);
                        break;
                    case FoodData.Type.KIMBAP:
                        AddIfEmpty(FoodsKimbap, food);
                        break;
                    case FoodData.Type.MEAL:
                        AddIfEmpty(FoodsMeal, food);
                        break;
                    case FoodData.Type.FLOURBASED:
                        AddIfEmpty(FoodsFlourbased, food);
                        break;
                    case FoodData.Type.PORKCUTLET:
                        AddIfEmpty(FoodsPorkcutlet, food);
                        break;
                    case FoodData.Type.SEASON:
                        AddIfEmpty(FoodsSeason, food);
                        break;
                    case FoodData.Type.UNDIFINED:
                        AddIfEmpty(FoodsUndified, food);
                        break;
                }
            }

            TypesTotalPrice[0] = GetTotalPrice(FoodsNew);
            TypesTotalPrice[1] = GetTotalPrice(FoodsKimbap);
            TypesTotalPrice[2] = GetTotalPrice(FoodsMeal);
            TypesTotalPrice[3] = GetTotalPrice(FoodsFlourbased);
            TypesTotalPrice[4] = GetTotalPrice(FoodsPorkcutlet);
            TypesTotalPrice[5] = GetTotalPrice(FoodsSeason);
            TypesTotalPrice[6] = GetTotalPrice(FoodsUndified);

            if (payType == PayType.Card)
            {
                PayTypeCount[0]++;
            }
            else
            {
                PayTypeCount[1]++;
            }
        }

        private static void AddIfEmpty(List<FoodData> target, FoodData data)
        {
            if (Utils.ContainsFoodDataByName(target, data.Name))
            {
                FoodData foodinlist = Utils.FindFoodDataByName(target, data.Name);
                foodinlist.Count += data.Count;
                foodinlist.Price += data.Price;
            }
            else
            {
                target.Add(data);
            }
        }

        public static int GetCateTotalPrice(FoodData.Type type)
        {
            int totalPrice = 0;

            switch (type)
            {
                case FoodData.Type.NEW:
                    totalPrice = TypesTotalPrice[0];
                    break;
                case FoodData.Type.KIMBAP:
                    totalPrice = TypesTotalPrice[1];
                    break;
                case FoodData.Type.MEAL:
                    totalPrice = TypesTotalPrice[2];
                    break;
                case FoodData.Type.FLOURBASED:
                    totalPrice = TypesTotalPrice[3];
                    break;
                case FoodData.Type.PORKCUTLET:
                    totalPrice = TypesTotalPrice[4];
                    break;
                case FoodData.Type.SEASON:
                    totalPrice = TypesTotalPrice[5];
                    break;
                case FoodData.Type.UNDIFINED:
                    totalPrice = TypesTotalPrice[6];
                    break;
            }

            return totalPrice;
        }

        public static int GetAllTotalPrice()
        {
            return AllTotalPrice + GetCurrentTotalPrice();
        }

        public static int GetCurrentTotalPrice()
        {
            int total = 0;
            foreach (int price in TypesTotalPrice)
            {
                total += price;
            }
            return total;
        }

        private static int GetTotalPrice(List<FoodData> foodDatas)
        {
            int totalPrice = 0;
            foreach (FoodData foodData in foodDatas)
            {
                totalPrice += foodData.Price;
            }

            return totalPrice;
        }

        public static int GetCateCount(FoodData.Type type)
        {
            int count = 0;

            switch (type)
            {
                case FoodData.Type.NEW:
                    count = GetFoodsCount(FoodsNew);
                    break;

                case FoodData.Type.KIMBAP:
                    count = GetFoodsCount(FoodsKimbap);
                    break;

                case FoodData.Type.MEAL:
                    count = GetFoodsCount(FoodsMeal);
                    break;

                case FoodData.Type.FLOURBASED:
                    count = GetFoodsCount(FoodsFlourbased);
                    break;

                case FoodData.Type.PORKCUTLET:
                    count = GetFoodsCount(FoodsPorkcutlet);
                    break;

                case FoodData.Type.SEASON:
                    count = GetFoodsCount(FoodsSeason);
                    break;

                case FoodData.Type.UNDIFINED:
                    count = GetFoodsCount(FoodsUndified);
                    break;
            }

            return count;
        }

        private static int GetFoodsCount(List<FoodData> foods)
        {
            int count = 0;

            foreach (FoodData food in foods)
            {
                count += food.Count;
            }

            return count;
        }

        public static List<FoodData> GetAllFoodDatas()
        {
            List<FoodData> statsFoodsAll = new List<FoodData>();
            statsFoodsAll.AddRange(FoodsNew);
            statsFoodsAll.AddRange(FoodsKimbap);
            statsFoodsAll.AddRange(FoodsMeal);
            statsFoodsAll.AddRange(FoodsFlourbased);
            statsFoodsAll.AddRange(FoodsSeason);
            statsFoodsAll.AddRange(FoodsUndified);

            return statsFoodsAll;
        }

        public static int GetPayTypeCount(PayType payType)
        {
            if (payType == PayType.Card)
            {
                return PayTypeCount[0];
            } else
            {
                return PayTypeCount[1];
            }
        }
        
        public static void SaveState()
        {
            Settings.PutInt("allTotalPrice", GetAllTotalPrice());
        }
    }
}
