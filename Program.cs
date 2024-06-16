using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using library_for_lab10;
using library_for_lab12;

namespace lab14
{
    public class Store
    {

        public string Name;
        public string City { get; set; }
        public int Rating { get; set; }

        public Store(string name, string city, int rating)
        {
            Name = name;
            City = city;
            Rating = rating;
        }
        public void Print()
        {
            Console.WriteLine($"Магазин: {Name} Город{City} Рейтинг{Rating}");
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Store other = (Store)obj;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class Program
    {
        static Queue<Tool> CreateToolQueue(int length)
        {
            var queue = new Queue<Tool>();
            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                switch (rnd.Next(3))
                {
                    case 0:
                        HandTool handTool = new HandTool();
                        handTool.RandomInit();
                        queue.Enqueue(handTool);
                        break;
                    case 1:
                        MeasuringTool measuringTool = new MeasuringTool();
                        measuringTool.RandomInit();
                        queue.Enqueue(measuringTool);
                        break;
                    case 2:
                        ElectricTool electricTool = new ElectricTool();
                        electricTool.RandomInit();
                        queue.Enqueue(electricTool);
                        break;
                }
            }
            return queue;
        }

        static void DisplayNetwork(SortedDictionary<string, Queue<Tool>> network)
        {
            foreach (var store in network)
            {
                Console.WriteLine($"\n\n{store.Key}:\n");
                DisplayToolStore(store.Value);
            }
        }

        static void DisplayNetwork(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> network)
        {
            foreach (var networkEntry in network)
            {
                Console.WriteLine($"\n\nNetwork: {networkEntry.Key}\n");
                DisplayStoreCollection(networkEntry.Value);
            }
        }

        static void DisplayToolStore(Queue<Tool> toolStore)
        {
            foreach (var tool in toolStore)
            {
                tool.VirtualPrint();
            }
        }

        static void DisplayStoreCollection(SortedDictionary<string, Queue<Tool>> storeCollection)
        {
            foreach (var store in storeCollection)
            {
                Console.WriteLine($"\nStore: {store.Key}\n");
                DisplayToolStore(store.Value);
            }
        }

        //ЧАСТЬ1

        //Where
        #region
        static void PerformWhereQueryLinq(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork5)
        {
            //выборка данных (Where) с использованием LINQ
            var ShortNameToolsLinq = from network in overallNetwork5
                                     from store in network.Value
                                     from tool in store.Value
                                     where (tool.Name).Length < 5
                                     select tool;

            Console.WriteLine("Инструменты у которых название меньше из 5 букв(LINQ):");
            foreach (var tool in ShortNameToolsLinq)
            {
                Console.WriteLine(tool);

            }
        }

        static void PerformWhereQueryExtensionMethods(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork5)
        {
            //выборка данных (Where) с использованием методов расширения
            var ToolsExtensionsWithShortName = overallNetwork5.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                     .Where(tool => (tool.Name).Length < 5);

            Console.WriteLine("\nИнструменты у которых название меньше из 5 букв (расширенный метод):");
            foreach (var tool in ToolsExtensionsWithShortName)
            {
                Console.WriteLine(tool);
            }
        }
        #endregion

        //Union
        #region
        static void PerformUnionQueryLinq(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var toolsInSet1 = overallNetwork["сеть1"].SelectMany(store => store.Value);
            var toolsInSet2 = overallNetwork["сеть2"].SelectMany(store => store.Value);

            var exceptToolsLinq = from tool in toolsInSet1.Union(toolsInSet2)
                                  select tool;

            Console.WriteLine("\nExcept tools (LINQ):");
            foreach (var tool in exceptToolsLinq)
            {
                Console.WriteLine(tool);
            }
        }

        static void PerformUnionQueryExtensionMethods(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var toolsInSet1 = overallNetwork["сеть1"].SelectMany(store => store.Value);
            var toolsInSet2 = overallNetwork["сеть2"].SelectMany(store => store.Value);

            var exceptToolsExtensions = toolsInSet1.Union(toolsInSet2);

            Console.WriteLine("\nExcept (Расширенный метод):");
            foreach (var tool in exceptToolsExtensions)
            {
                Console.WriteLine(tool);

            }
        }
        #endregion

        //Max
        #region
        static void PerformMaxQueryLinq(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var maxBatteryTimeLinq = (from network in overallNetwork
                                      from store in network.Value
                                      from tool in store.Value
                                      where tool is ElectricTool
                                      select (ElectricTool)tool).Max(tool => tool.BatteryTime);

            Console.WriteLine($"\nМаксимальное время работы батареи (LINQ): {maxBatteryTimeLinq}");
        }

        static void PerformMaxQueryExtensionMethods(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var maxBatteryTimeExtensions = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                         .OfType<ElectricTool>()
                                                         .Max(tool => tool.BatteryTime);

            Console.WriteLine($"\nМаксимальное время работы батареи (расширенный метод): {maxBatteryTimeExtensions}");
        }
        #endregion

        //Group by
        #region
        static void GroupByQueryLinq(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var toolsGroupedByTypeLinq = from network in overallNetwork
                                         from store in network.Value
                                         from tool in store.Value
                                         group tool by tool.GetType().Name into typeGroup
                                         select new
                                         {
                                             ToolType = typeGroup.Key,
                                             Count = typeGroup.Count(),
                                             Tools = typeGroup.ToList()
                                         };

            Console.WriteLine("\nГруппировка по типу инструмента (LINQ):");
            foreach (var group in toolsGroupedByTypeLinq)
            {
                Console.WriteLine($"\nТип инструмента: {group.ToolType}, Количество: {group.Count}");
                foreach (var tool in group.Tools)
                {
                    tool.VirtualPrint();
                }
            }
        }

        static void GroupByQueryExtensionMethods(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var toolsGroupedByTypeExtensions = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                             .GroupBy(tool => tool.GetType().Name)
                                                             .Select(typeGroup => new
                                                             {
                                                                 ToolType = typeGroup.Key,
                                                                 Count = typeGroup.Count(),
                                                                 Tools = typeGroup.ToList()
                                                             });

            Console.WriteLine("\nГруппировка по типу инструмента (расширенный метод):");
            foreach (var group in toolsGroupedByTypeExtensions)
            {
                Console.WriteLine($"\nТип инструмента: {group.ToolType}, Количество: {group.Count}");
                foreach (var tool in group.Tools)
                {
                    tool.VirtualPrint();
                }
            }
        }
        #endregion

        //Let (для расчёта стоимости инструмента)
        #region
        static void PerformLetQueryLinq(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var toolsWithCalculatedCostLinq = from network in overallNetwork
                                              from store in network.Value
                                              from tool in store.Value
                                              let cost = CalculateCost(tool)
                                              select new { Tool = tool, Cost = cost };

            Console.WriteLine("\nИнструменты с рассчитанной стоимостью (LINQ):");
            foreach (var entry in toolsWithCalculatedCostLinq)
            {
                Console.WriteLine($"Инструмент: {entry.Tool.Name}, Стоимость: {entry.Cost}");
            }
        }

        static void PerformLetQueryExtensionMethods(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork)
        {
            var toolsWithCalculatedCostExtensions = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                                 .Select(tool => new { Tool = tool, Cost = CalculateCost(tool) });

            var expensiveTools = toolsWithCalculatedCostExtensions.Where(t => t.Cost > 500); // Пример использования промежуточной переменной

            Console.WriteLine("\nДорогие инструменты(1000р) (расширенный метод):");
            foreach (var tool in expensiveTools)
            {
                Console.WriteLine($"Инструмент: {tool.Tool.Name}, Стоимость: {tool.Cost}");
            }
        }

        static int CalculateCost(Tool tool)
        {
            int cost = 0;
            if (tool.GetType() == typeof(HandTool))
            {
                cost = 1000;
            }
            else if (tool.GetType() == typeof(MeasuringTool))
            {
                cost = 200;
            }
            else if (tool.GetType() == typeof(ElectricTool))
            {
                cost = 300;
            }
            return cost;
        }
        #endregion

        //join
        #region
        //linq
        static void JoinLINQ(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork, Queue<Store> stores)
        {
            var res10 = from network in overallNetwork
                        from store in network.Value
                        from tool in store.Value
                        where tool is HandTool
                        join t in stores on tool.Name equals t.Name
                        select $"Инструмент: {tool.Name} - Город: {t.City} | Рейтинг: {t.Rating}";

            Console.WriteLine("Join (LINQ):");
            foreach (var item in res10)
            {
                Console.WriteLine(item);
            }
        }

        //с использованием расширенного метода
        static void JoinExtensionMethod(SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork, Queue<Store> stores)
        {
            var res10 = overallNetwork.SelectMany(network => network.Value)
                                      .SelectMany(store => store.Value)
                                      .OfType<HandTool>()
                                      .Join(stores, tool => tool.Name, store => store.Name,
                                            (tool, store) => $"Инструмент: {tool.Name} - Город: {store.City} | Рейтинг: {store.Rating}");

            Console.WriteLine("Join (Extension Method):");
            foreach (var item in res10)
            {
                Console.WriteLine(item);
            }
        }
        #endregion

        //ЧАСТЬ2

        //Where
        #region
        static void PerformWhereQueryLinqForLab14(MyCollection<Tool> lab14)
        {
            var whereQueryLinq = from tool in lab14
                                 where tool.Name.Length > 8
                                 select tool;

            Console.WriteLine("\nИнструменты с названием более 8 символов (LINQ):");
            foreach (var tool in whereQueryLinq)
            {
                Console.WriteLine(tool);
            }
        }

        //Where (Метод расширений)
        static void PerformWhereQueryExtensionForLab14(MyCollection<Tool> lab14)
        {
            var whereQueryExtension = lab14.Where(tool => tool.Name.Length > 8);

            Console.WriteLine("\nИнструменты с названием более 8 символов (методы расширений):");
            foreach (var tool in whereQueryExtension)
            {
                Console.WriteLine(tool);
            }
        }
        #endregion

        //Count
        #region
        //(LINQ)
        static void PerformCountQueryLinqForLab14(MyCollection<Tool> lab14)
        {
            var countQueryLinq = (from tool in lab14
                                  where tool.Name.Length > 8
                                  select tool).Count();

            Console.WriteLine($"\nКоличество инструментов с названием более 8 символов (LINQ): {countQueryLinq}");
        }

        //Метод расширений
        static void PerformCountQueryExtensionForLab14(MyCollection<Tool> lab14)
        {
            var countQueryExtension = lab14.Count(tool => tool.Name.Length > 8);

            Console.WriteLine($"\nКоличество инструментов с названием более 8 символов (методы расширений): {countQueryExtension}");
        }
        #endregion

        //Min
        #region
        //LINQ
        static void PerformMinLinq12(MyCollection<Tool> lab12)
        {
            var averageAccuracyLinq = (from tool in lab12
                                       where tool is MeasuringTool
                                       select (MeasuringTool)tool).Min(tool => tool.Accuracy);

            Console.WriteLine($"\nМинимальная точность измерительных инстурментов(LINQ): {averageAccuracyLinq}");
        }

        //
        static void PerformMinExtensionMethod(MyCollection<Tool> lab12)
        {
            var averageAccuracyExtensions = lab12.OfType<MeasuringTool>().Min(tool => tool.Accuracy);

            Console.WriteLine($"\nМинимальная точность измерительных инстурментов(расширенный метод): {averageAccuracyExtensions}");
        }

        #endregion

        //Group by
        #region
        //LINQ
        static void GroupByQueryLinq12(MyCollection<Tool> lab14)
        {
            var toolsGroupedByTypeLinq = from tool in lab14
                                         group tool by tool.GetType().Name into typeGroup
                                         select new
                                         {
                                             ToolType = typeGroup.Key,
                                             Count = typeGroup.Count(),
                                             Tools = typeGroup.ToList()
                                         };

            Console.WriteLine("\nГруппировка по типу инструмента (LINQ):");
            foreach (var group in toolsGroupedByTypeLinq)
            {
                Console.WriteLine($"\nТип инструмента: {group.ToolType}, Количество: {group.Count}");
                foreach (var tool in group.Tools)
                {
                    Console.WriteLine(tool);
                }
            }
        }

        static void GroupByQueryExtensionMethods12(MyCollection<Tool> lab14)
        {
            var toolsGroupedByTypeExtensions = lab14.GroupBy(tool => tool.GetType().Name)
                                                   .Select(typeGroup => new
                                                   {
                                                       ToolType = typeGroup.Key,
                                                       Count = typeGroup.Count(),
                                                       Tools = typeGroup.ToList()
                                                   });

            Console.WriteLine("\nГруппировка по типу инструмента (расширенный метод):");
            foreach (var group in toolsGroupedByTypeExtensions)
            {
                Console.WriteLine($"\nТип инструмента: {group.ToolType}, Количество: {group.Count}");
                foreach (var tool in group.Tools)
                {
                    Console.WriteLine(tool);
                }
            }
        }
        #endregion


        static void Main(string[] args)
        {
            //тип 1 - словарь сеть магазинов
            //тип 2 - магазин инстурментов очередь

            //создание магазинов для сети магазинов 1
            var shop1 = CreateToolQueue(10);
            var shop2 = CreateToolQueue(2);
            shop2.Enqueue(new HandTool("отвёртка", "МРУ", 22));
            shop2.Enqueue(new HandTool("пассатижи", "МРУ", 33));
            shop2.Enqueue(new HandTool("молоток", "МРУ", 33));
            var shop3 = CreateToolQueue(5);

            //создание магазинов для сети магазинов 2
            var shop4 = CreateToolQueue(10);
            var shop5 = CreateToolQueue(2);
            var shop6 = CreateToolQueue(5);

            //создание сети магазинов 1
            var network1 = new SortedDictionary<string, Queue<Tool>>();
            network1.Add("магазин1_1", new Queue<Tool>(shop1));
            network1.Add("магазин1_2", new Queue<Tool>(shop2));
            network1.Add("магазин1_3", new Queue<Tool>(shop3));

            //создание сети магазинов 2
            var network2 = new SortedDictionary<string, Queue<Tool>>();
            network2.Add("магазин2_1", new Queue<Tool>(shop4));
            network2.Add("магазин2_2", new Queue<Tool>(shop5));
            network2.Add("магазин2_3", new Queue<Tool>(shop6));

            //создание общей коллекции, содержащей сети магазинов
            var overallNetwork = new SortedDictionary<string, SortedDictionary<string, Queue<Tool>>>();
            overallNetwork.Add("сеть1", network1);
            overallNetwork.Add("сеть2", network2);


            //часть2
            MyCollection<Tool> lab14 = new MyCollection<Tool>(10);
            MeasuringTool measuringTool = new MeasuringTool("инструмент", "материал1", "См", 202.2, 22);
            lab14.Add(measuringTool);

            //Главное меню программы
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Главное меню:");
                Console.WriteLine("                                       ЧАСТЬ 1");
                Console.WriteLine("1. Магазин из 10 инструментов");
                Console.WriteLine("2. Магазин из 2 инструментов");
                Console.WriteLine("3. Магазин из 5 инструментов");
                Console.WriteLine("4. Сеть магазинов 1");
                Console.WriteLine("5. Сеть магазинов 2");
                Console.WriteLine("6. Отображение общей коллекции");
                Console.WriteLine("7. Where");
                Console.WriteLine("8. Except");
                Console.WriteLine("9. Max");
                Console.WriteLine("10. Group by");
                Console.WriteLine("11. Let");
                Console.WriteLine("12. Join");
                Console.WriteLine("                                       ЧАСТЬ 2");
                Console.WriteLine("13. Коллекция из 12лаб");
                Console.WriteLine("14. Where");
                Console.WriteLine("15. Count");
                Console.WriteLine("16. Average");
                Console.WriteLine("17. Group by");


                Console.Write("Выберите действие: ");

                int choice;
                if (int.TryParse(Console.ReadLine(), out choice))
                {

                    switch (choice)
                    {
                        case 1:
                            Console.WriteLine("\n\n Магазин из 10 инструментов:\n");
                            DisplayToolStore(shop1);
                            break;
                        case 2:
                            Console.WriteLine("\n\n Магазин из 2 инструментов:\n");
                            DisplayToolStore(shop2);
                            break;
                        case 3:
                            Console.WriteLine("\n\n Магазин из 5 инструментов:\n");
                            DisplayToolStore(shop3);
                            break;
                        case 4:
                            Console.WriteLine();
                            DisplayNetwork(network1);
                            break;
                        case 5:
                            DisplayNetwork(network2);
                            break;
                        case 6:
                            //oтображение общей коллекции
                            DisplayNetwork(overallNetwork);
                            break;
                        case 7:
                            //WHERE
                            PerformWhereQueryExtensionMethods(overallNetwork);
                            PerformWhereQueryLinq(overallNetwork);
                            break;
                        case 8:
                            //EXCEPT(Оператор EXCEPT позволяет найти разность двух выборок, то есть те строки которые есть в первой выборке, но которых нет во второй)
                            PerformUnionQueryExtensionMethods(overallNetwork);
                            PerformUnionQueryLinq(overallNetwork);
                            break;
                        case 9:
                            //Max
                            ElectricTool electricTool = new ElectricTool("специально добавленный инстуремнт", "материал", "аккумулятор", 2.2, 2);
                            shop1.Enqueue(electricTool);
                            PerformMaxQueryExtensionMethods(overallNetwork);
                            PerformMaxQueryLinq(overallNetwork);
                            break;
                        case 10:
                            //Group by
                            GroupByQueryExtensionMethods(overallNetwork);
                            GroupByQueryLinq(overallNetwork);
                            break;
                        case 11:
                            //Let
                            PerformLetQueryExtensionMethods(overallNetwork);
                            PerformLetQueryLinq(overallNetwork);
                            break;
                        case 12:
                            //JOIN
                            Queue<Store> stores = new Queue<Store>();
                            Store s1 = new Store("отвёртка", "Пермь", 2);
                            Store s2 = new Store("пассатижи", "Москва", 17);
                            Store s3 = new Store("молоток", "СПБ", 35);
                            stores.Enqueue(s1);
                            stores.Enqueue(s2);
                            stores.Enqueue(s3);

                            JoinLINQ(overallNetwork, stores);
                            JoinExtensionMethod(overallNetwork, stores);

                            break;
                        case 13:
                            Console.WriteLine("\nКоллекция из 12лаб.");
                            lab14.PrintList();
                            break;
                        case 14:
                            //Where
                            PerformWhereQueryExtensionForLab14(lab14);
                            PerformWhereQueryLinqForLab14(lab14);
                            break;
                        case 15:
                            //Count
                            PerformCountQueryExtensionForLab14(lab14);
                            PerformCountQueryLinqForLab14(lab14);
                            break;
                        case 16:
                            //Min
                            MeasuringTool measiringTool = new MeasuringTool("измерительный инструмент", "материал1", "см", 10.05, 22);
                            lab14.Add(measiringTool);
                            PerformMinExtensionMethod(lab14);
                            PerformMinLinq12(lab14);
                            break;
                        case 17:
                            //GroupBy
                            GroupByQueryLinq12(lab14);
                            GroupByQueryExtensionMethods12(lab14);
                            break;
                        case 0:
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Некорректный ввод. Попробуйте снова.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Попробуйте снова.");
                }

                Console.WriteLine();




            }
        }
    }
}