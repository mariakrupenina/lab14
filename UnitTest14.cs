using lab14;
using library_for_lab10;
using library_for_lab12;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest14
{
    [TestClass]
    public class UnitTest14
    {
        private SortedDictionary<string, SortedDictionary<string, Queue<Tool>>> overallNetwork;
        private Queue<Store> stores;
        private MyCollection<Tool> lab14Collection;

        [TestInitialize]
        public void Setup()
        {
            overallNetwork = new SortedDictionary<string, SortedDictionary<string, Queue<Tool>>>();

            // Network 1
            var network1 = new SortedDictionary<string, Queue<Tool>>();
            var shop1 = new Queue<Tool>();
            shop1.Enqueue(new HandTool("отвёртка", "материал1", 22));
            shop1.Enqueue(new MeasuringTool("линейка", "металл", "см", 1.0, 95));
            shop1.Enqueue(new ElectricTool("дрель", "пластик", "аккумулятор", 3.0, 60));
            network1.Add("магазин1_1", shop1);

            overallNetwork.Add("сеть1", network1);

            // Network 2
            var network2 = new SortedDictionary<string, Queue<Tool>>();
            var shop2 = new Queue<Tool>();
            shop2.Enqueue(new HandTool("пассатижи", "материал2", 33));
            network2.Add("магазин2_1", shop2);

            overallNetwork.Add("сеть2", network2);

            stores = new Queue<Store>();
            stores.Enqueue(new Store("отвёртка", "Пермь", 2));
            stores.Enqueue(new Store("пассатижи", "Москва", 17));

            lab14Collection = new MyCollection<Tool>(10);
            lab14Collection.Add(new MeasuringTool("инструмент", "материал1", "См", 202.2, 22));
            lab14Collection.Add(new HandTool("random", "материал1", 12));
            lab14Collection.Add(new ElectricTool("random", "материал1", "Батареи", 12.4, 24));
        }

        [TestMethod]
        public void PerformWhereQueryLinqTest()
        {
            var result = from network in overallNetwork
                         from store in network.Value
                         from tool in store.Value
                         where tool.Name.Length < 9
                         select tool;

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void PerformWhereQueryExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .Where(tool => tool.Name.Length < 9);

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void PerformUnionQueryLinqTest()
        {
            var toolsInSet1 = overallNetwork["сеть1"].SelectMany(store => store.Value);
            var toolsInSet2 = overallNetwork["сеть2"].SelectMany(store => store.Value);

            var result = from tool in toolsInSet1.Union(toolsInSet2)
                         select tool;

            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void PerformUnionQueryExtensionMethodsTest()
        {
            var toolsInSet1 = overallNetwork["сеть1"].SelectMany(store => store.Value);
            var toolsInSet2 = overallNetwork["сеть2"].SelectMany(store => store.Value);

            var result = toolsInSet1.Union(toolsInSet2);

            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void PerformMaxQueryLinqTest()
        {
            var result = (from network in overallNetwork
                          from store in network.Value
                          from tool in store.Value
                          where tool is ElectricTool
                          select (ElectricTool)tool).Max(tool => tool.BatteryTime);

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void PerformMaxQueryExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .OfType<ElectricTool>()
                                       .Max(tool => tool.BatteryTime);

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void GroupByQueryLinqTest()
        {
            var result = from network in overallNetwork
                         from store in network.Value
                         from tool in store.Value
                         group tool by tool.GetType().Name into typeGroup
                         select new
                         {
                             ToolType = typeGroup.Key,
                             Count = typeGroup.Count(),
                             Tools = typeGroup.ToList()
                         };

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void GroupByQueryExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .GroupBy(tool => tool.GetType().Name)
                                       .Select(typeGroup => new
                                       {
                                           ToolType = typeGroup.Key,
                                           Count = typeGroup.Count(),
                                           Tools = typeGroup.ToList()
                                       });

            Assert.AreEqual(3, result.Count());
        }

        [TestMethod]
        public void PerformLetQueryLinqTest()
        {
            var result = from network in overallNetwork
                         from store in network.Value
                         from tool in store.Value
                         let cost = CalculateCost(tool)
                         select new { Tool = tool, Cost = cost };

            Assert.AreEqual(4, result.Count());
        }

        private int CalculateCost(Tool tool)
        {
            int cost = 0;
            if (tool is HandTool)
            {
                cost = 1000;
            }
            else if (tool is MeasuringTool)
            {
                cost = 200;
            }
            else if (tool is ElectricTool)
            {
                cost = 300;
            }
            return cost;
        }

        [TestMethod]
        public void PerformLetQueryExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .Select(tool => new { Tool = tool, Cost = CalculateCost(tool) });

            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void JoinLinqTest()
        {
            var result = from network in overallNetwork
                         from store in network.Value
                         from tool in store.Value
                         where tool is HandTool
                         join t in stores on tool.Name equals t.Name
                         select $"Инструмент: {tool.Name} - Город: {t.City} | Рейтинг: {t.Rating}";

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void JoinExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value)
                                       .SelectMany(store => store.Value)
                                       .OfType<HandTool>()
                                       .Join(stores, tool => tool.Name, store => store.Name,
                                            (tool, store) => $"Инструмент: {tool.Name} - Город: {store.City} | Рейтинг: {store.Rating}");

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void PerformWhereQueryLinqForLab14Test()
        {
            var result = from tool in lab14Collection
                         where tool.Name.Length > 8
                         select tool;

            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void PerformWhereQueryExtensionForLab14Test()
        {
            var result = lab14Collection.Where(tool => tool.Name.Length > 8);

            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void PerformCountQueryLinqForLab14Test()
        {
            var result = (from tool in lab14Collection
                          where tool.Name.Length > 8
                          select tool).Count();

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void PerformCountQueryExtensionForLab14Test()
        {
            var result = lab14Collection.Count(tool => tool.Name.Length > 8);

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void PerformMinLinq12Test()
        {
            var result = (from tool in lab14Collection
                          where tool is MeasuringTool
                          select (MeasuringTool)tool).Min(tool => tool.Accuracy);

            Assert.AreEqual(202.2, result);
        }

        [TestMethod]
        public void PerformMinExtensionMethodTest()
        {
            var result = lab14Collection.OfType<MeasuringTool>().Min(tool => tool.Accuracy);

            Assert.AreEqual(202.2, result);
        }

        [TestMethod]
        public void GroupByQueryLinq12Test()
        {
            var result = from tool in lab14Collection
                         group tool by tool.GetType().Name into typeGroup
                         select new
                         {
                             ToolType = typeGroup.Key,
                             Count = typeGroup.Count(),
                             Tools = typeGroup.ToList()
                         };

            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void GroupByQueryExtensionMethods12Test()
        {
            var result = lab14Collection.GroupBy(tool => tool.GetType().Name)
                                        .Select(typeGroup => new
                                        {
                                            ToolType = typeGroup.Key,
                                            Count = typeGroup.Count(),
                                            Tools = typeGroup.ToList()
                                        });

            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public void PerformAverageQueryLinqTest()
        {
            var result = (from network in overallNetwork
                          from store in network.Value
                          from tool in store.Value
                          where tool is ElectricTool
                          select (ElectricTool)tool).Average(tool => tool.BatteryTime);

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void PerformAverageQueryExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .OfType<ElectricTool>()
                                       .Average(tool => tool.BatteryTime);

            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void PerformAnyQueryLinqTest()
        {
            bool result = (from network in overallNetwork
                           from store in network.Value
                           from tool in store.Value
                           where tool.Name == "дрель"
                           select tool).Any();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PerformAnyQueryExtensionMethodsTest()
        {
            bool result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .Any(tool => tool.Name == "дрель");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PerformElementAtQueryLinqTest()
        {
            var result = (from network in overallNetwork
                          from store in network.Value
                          from tool in store.Value
                          select tool).ElementAt(2);

            Assert.AreEqual("дрель", result.Name);
        }

        [TestMethod]
        public void PerformElementAtQueryExtensionMethodsTest()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .ElementAt(2);

            Assert.AreEqual("дрель", result.Name);
        }

        [TestMethod]
        public void TestAddToolToNetwork()
        {
            // Добавление нового инструмента в сеть 1
            var newTool = new HandTool("гаечный ключ", "сталь", 25);
            overallNetwork["сеть1"]["магазин1_1"].Enqueue(newTool);

            Assert.IsTrue(overallNetwork["сеть1"]["магазин1_1"].Contains(newTool));
        }

        [TestMethod]
        public void TestStoreSearchByToolName()
        {
            var searchTool = "отвёртка";
            var result = stores.Where(store => store.Name == searchTool);

            Assert.AreEqual(1, result.Count());
        }

        [TestMethod]
        public void TestLab14CollectionAdd()
        {
            var newTool = new ElectricTool("газонокосилка", "пластик", "аккумулятор", 4.5, 45);
            lab14Collection.Add(newTool);

            Assert.IsTrue(lab14Collection.Contains(newTool));
        }

        [TestMethod]
        public void TestElectricToolBatteryType()
        {
            var electricTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                              .OfType<ElectricTool>();

            foreach (var tool in electricTools)
            {
                Assert.IsNotNull(tool.BatteryTime); // Убедимся, что у всех электрических инструментов есть тип батареи
            }
        }

        [TestMethod]
        public void TestFindNonExistentTool()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .Any(tool => tool.Name == "несуществующий инструмент");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestElectricToolNoBatteryTime()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .OfType<ElectricTool>()
                                       .Any(tool => tool.BatteryTime == 0);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestStoresInEmptyNetwork()
        {
            var emptyNetwork = new SortedDictionary<string, SortedDictionary<string, Queue<Tool>>>();

            var storeCount = emptyNetwork.SelectMany(network => network.Value).Count();

            Assert.AreEqual(0, storeCount);
        }

        [TestMethod]
        public void TestAddMultipleToolsToNetwork()
        {
            var newTools = new List<Tool>
            {
                new HandTool("молоток", "сталь", 30),
                new MeasuringTool("штангенциркуль", "металл", "мм", 0.1, 200)
            };

            foreach (var tool in newTools)
            {
                overallNetwork["сеть1"]["магазин1_1"].Enqueue(tool);
            }

            Assert.AreEqual(5, overallNetwork["сеть1"]["магазин1_1"].Count);
        }

        [TestMethod]
        public void TestSearchForToolInStores()
        {
            var toolName = "пассатижи";
            var storeWithTool = stores.FirstOrDefault(store => store.Name == toolName);

            Assert.IsNotNull(storeWithTool);
            Assert.AreEqual("Москва", storeWithTool.City);
        }

        [TestMethod]
        public void TestStoreNotFound()
        {
            var toolName = "несуществующий инструмент";
            var storeWithTool = stores.FirstOrDefault(store => store.Name == toolName);

            Assert.IsNull(storeWithTool);
        }

        [TestMethod]
        public void TestRemoveNonExistentTool()
        {
            var nonExistentTool = new HandTool("non-existent", "material", 10);
            bool result = lab14Collection.Remove(nonExistentTool);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestFindAllHandTools()
        {
            var handTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                          .OfType<HandTool>();

            Assert.AreEqual(2, handTools.Count());
        }

        [TestMethod]
        public void TestFindAllElectricTools()
        {
            var electricTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                              .OfType<ElectricTool>();

            Assert.AreEqual(1, electricTools.Count());
        }

        [TestMethod]
        public void TestAverageAccuracyMeasuringTools()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .OfType<MeasuringTool>()
                                       .Average(tool => tool.Accuracy);

            Assert.AreEqual(95.0, result);
        }

        [TestMethod]
        public void TestAverageBatteryTimeLab14Collection()
        {
            var result = lab14Collection.OfType<ElectricTool>().Average(tool => tool.BatteryTime);

            Assert.AreEqual(18.2, result);
        }

        [TestMethod]
        public void TestMaxRatingStores()
        {
            var result = stores.Max(store => store.Rating);

            Assert.AreEqual(17, result);
        }

        [TestMethod]
        public void TestCountMeasuringTools()
        {
            var result = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                       .OfType<MeasuringTool>()
                                       .Count();

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestCountToolsByType()
        {
            var toolCountsByType = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                 .GroupBy(tool => tool.GetType().Name)
                                                 .Select(group => new { ToolType = group.Key, Count = group.Count() });

            var handToolCount = toolCountsByType.FirstOrDefault(group => group.ToolType == "HandTool")?.Count ?? 0;
            var measuringToolCount = toolCountsByType.FirstOrDefault(group => group.ToolType == "MeasuringTool")?.Count ?? 0;
            var electricToolCount = toolCountsByType.FirstOrDefault(group => group.ToolType == "ElectricTool")?.Count ?? 0;

            Assert.AreEqual(2, handToolCount);
            Assert.AreEqual(1, measuringToolCount);
            Assert.AreEqual(1, electricToolCount);
        }

        [TestMethod]
        public void TestGroupByToolTypeLab14Collection()
        {
            var groupedTools = lab14Collection.GroupBy(tool => tool.GetType().Name)
                                              .Select(group => new { ToolType = group.Key, Count = group.Count() });

            Assert.AreEqual(4, groupedTools.Count());
        }

        [TestMethod]
        public void TestFindFirstMeasuringToolLab14Collection()
        {
            var firstMeasuringTool = lab14Collection.OfType<MeasuringTool>().FirstOrDefault();

            Assert.IsNotNull(firstMeasuringTool);
            Assert.AreEqual("инструмент", firstMeasuringTool.Name);
        }

        [TestMethod]
        public void TestFindAllMeasuringToolsLab14Collection()
        {
            var measuringToolsCount = lab14Collection.OfType<MeasuringTool>().Count();

            Assert.AreEqual(1, measuringToolsCount);
        }

        
        [TestMethod]
        public void TestOverallNetworkContainsKey()
        {
            bool result = overallNetwork.ContainsKey("сеть1");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestLab14CollectionClear()
        {
            lab14Collection.Clear();

            Assert.AreEqual(0, lab14Collection.Count);
        }

        [TestMethod]
        public void TestHandToolMaterial()
        {
            var handTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                          .OfType<HandTool>();

            foreach (var tool in handTools)
            {
                Assert.IsNotNull(tool.Material); // Убедимся, что у всех ручных инструментов есть материал
            }
        }

        [TestMethod]
        public void TestMeasuringToolUnits()
        {
            var measuringTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                               .OfType<MeasuringTool>();

            foreach (var tool in measuringTools)
            {
                Assert.IsNotNull(tool.MeasurementUnits); // Убедимся, что у всех измерительных инструментов есть единицы измерения
            }
        }

        [TestMethod]
        public void TestElectricToolBatteryType2()
        {
            var electricTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                              .OfType<ElectricTool>();

            foreach (var tool in electricTools)
            {
                Assert.IsNotNull(tool.BatteryTime); // Убедимся, что у всех электрических инструментов есть тип батареи
            }
        }

        [TestMethod]
        public void TestHandToolMateria2l()
        {
            var handTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                          .OfType<HandTool>();

            foreach (var tool in handTools)
            {
                Assert.IsNotNull(tool.Material); // Убедимся, что у всех ручных инструментов есть материал
            }
        }

        [TestMethod]
        public void TestMeasuringToolUnits2()
        {
            var measuringTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                               .OfType<MeasuringTool>();

            foreach (var tool in measuringTools)
            {
                Assert.IsNotNull(tool.MeasurementUnits); // Убедимся, что у всех измерительных инструментов есть единицы измерения
            }
        }

        [TestMethod]
        public void TestFilterByCity()
        {
            var toolsInMoscow = stores.Where(store => store.City == "Москва").Select(store => store.Name).ToList();

            var filteredTools = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                              .Where(tool => toolsInMoscow.Contains(tool.Name));

            foreach (var tool in filteredTools)
            {
                Assert.AreEqual("Москва", stores.First(store => store.Name == tool.Name).City);
            }
        }

        [TestMethod]
        public void TestFindFirstElectricTool()
        {
            var firstElectricTool = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                  .OfType<ElectricTool>()
                                                  .FirstOrDefault();

            Assert.IsNotNull(firstElectricTool);
            Assert.AreEqual("дрель", firstElectricTool.Name);
        }

        [TestMethod]
        public void TestSumBatteryTime()
        {
            var totalBatteryTime = overallNetwork.SelectMany(network => network.Value.SelectMany(store => store.Value))
                                                 .OfType<ElectricTool>()
                                                 .Sum(tool => tool.BatteryTime);

            Assert.AreEqual(3, totalBatteryTime); // Сумма времени работы батарей всех электрических инструментов
        }

        [TestMethod]
        public void TestStoresInNetwork()
        {
            var network1Stores = overallNetwork["сеть1"].Count;
            var network2Stores = overallNetwork["сеть2"].Count;

            Assert.AreEqual(1, network1Stores);
            Assert.AreEqual(1, network2Stores);
        }

        [TestMethod]
        public void TestMeasuringToolsWithHighAccuracy()
        {
            var highAccuracyTools = lab14Collection.OfType<MeasuringTool>().Where(tool => tool.Accuracy > 100);

            Assert.AreEqual(1, highAccuracyTools.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(System.NullReferenceException))]
        public void TestAddNullToolToCollection()
        {
            lab14Collection.Add(null);
        }

        [TestMethod]
        public void TestAddDuplicateTool()
        {
            var toolToAdd = lab14Collection.FirstOrDefault();
            lab14Collection.Add(toolToAdd);

            Assert.AreEqual(2, lab14Collection.Count(t => t.Equals(toolToAdd)));
        }

        
        private bool IsSorted(IEnumerable<Tool> collection)
        {
            var sorted = collection.OrderBy(tool => tool.Name);
            return collection.SequenceEqual(sorted);
        }

        [TestMethod]
        public void TestLab14CollectionClear2()
        {
            //Arrange & Act
            lab14Collection.Clear();

            //Assert
            Assert.AreEqual(0, lab14Collection.Count());
        }

    }
}
