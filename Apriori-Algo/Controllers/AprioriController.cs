using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.MachineLearning;
using System.IO;
using CsvHelper;
using System.Globalization;
using  Microsoft.AspNetCore.Hosting;
using  Microsoft.Extensions.Hosting;
using Apriori_Algo.Models;

namespace Apriori_Algo.Controllers
{

    public class AprioriController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly MyDbContext c;
        public AprioriController(MyDbContext m,IWebHostEnvironment hostingEnvironment)
        {
            c = m;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index(double? support, double? confidence)
        {
            // استخدام القيم الافتراضية لحد الدعم وحد الثقة في حال عدم تمرير قيم
            double defaultSupport = 0.01;
            double defaultConfidence = 0.03;
            double actualSupport = support ?? defaultSupport;
            double actualConfidence = confidence ?? defaultConfidence;
            // استخدام القيم الممررة من المستخدم في حدود الدعم والثقة
            ViewBag.Support = actualSupport;
            ViewBag.Confidence = actualConfidence;
        

            // تحديد مسار ملف CSV

            string csvFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "data2.csv");



            // قراءة ملف CSV وتحويله إلى مصفوفة من القيم الرقمية
            var data = new List<int[]>();
            using (var reader = new StreamReader(csvFilePath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    int value1;
                    int value2;

                    if (int.TryParse(values[0], out value1) && int.TryParse(values[1], out value2))
                    {
                        var row = new int[2];
                        row[0] = value1;
                        row[1] = value2;

                        data.Add(row);
                        ViewBag.d = data;
                     
                    }
                    else
                    {
                        // معالجة القيم غير الصحيحة
                    }
                }
            }
           
            // إنشاء قائمة لجميع العناصر المستخدمة في البيانات
            var items = new SortedSet<int>();
            foreach (var row in data)
            {
                foreach (var item in row)
                {
                    items.Add(item);
                }
            }
            ViewBag.item = items;

            // إزالة العناصر المكررة من البيانات الأصلية
            var distinctData = new List<int[]>();
            foreach (var row in data)
            {
                var distinctRow = row.Distinct().ToArray();
                distinctData.Add(distinctRow);
            }

            // إنشاء قائمة لتخزين جميع المجموعات المحتملة من العناصر
            var allItemSets = new List<int[]>();
            for (int i = 2; i <= items.Count; i++)
            {
                var itemSetsOfSizeI = Combinations(items, i);
                allItemSets.AddRange(itemSetsOfSizeI);
            }

            // إنشاء قائمة لتخزين العناصر التي تم العثور عليها
            var itemSets = new List<int[]>();

            // حساب عدد الظهور لكل مجموعة فرعية من العناصر في البيانات
            foreach (var itemSet in allItemSets)
            {
                int count = 0;

                foreach (var row in distinctData)
                {
                    if (itemSet.All(row.Contains))
                    {
                        count++;
                    }
                }

                if ((double)count / distinctData.Count >= support)
                {
                    itemSets.Add(itemSet);
                }
            }

            ViewBag.set = itemSets;



            // استخدام القواعد المؤهلة وحساب عددها في البيانات
            int k = 2;
            while (itemSets.Any(s => s.Length == k - 1))
            {
                var qualifiedSets = new List<int[]>();

                foreach (var set1 in itemSets)
                {
                    foreach (var set2 in itemSets)
                    {
                        if (set1.Intersect(set2).Count() == k - 2 && !set1.SequenceEqual(set2))
                        {
                            var newSet = set1.Union(set2).ToArray();

                            if (!qualifiedSets.Any(s => s.SequenceEqual(newSet)))
                            {
                                qualifiedSets.Add(newSet);
                            }
                        }
                    }
                }
         
             var frequentSets = new List<int[]>();
                foreach (var set in qualifiedSets)
                {
                    int count = 0;

                    foreach (var row in data)
                    {
                        if (set.All(s => row.Contains(s)))
                        {
                            count++;
                        }
                    }

                    if ((double)count / data.Count >= support)
                    {
                        frequentSets.Add(set);
                    }
                }

                itemSets.AddRange(frequentSets);
                k++;
            }
            ViewBag.i = itemSets;

          
              
            // حساب قيمة الثقة للقواعد المؤهلة واختيار تلك التي تحقق حد الثقة المطلوب
            var rules = new List<int[]>();
            foreach (var set in itemSets)
            {
                if (set.Length > 1)
                {
                    for (int i = 1; i < set.Length; i++)
                    {
                        var subsets = Combinations(new SortedSet<int>(set), i);
                        foreach (var subset in subsets)
                        {
                            var remaining = new SortedSet<int>(set.Except(subset).ToArray());
                            double confidenceValue = (double)CountOccurences(data, set) / CountOccurences(data, subset);

                            if (confidenceValue >= confidence)
                            {
                                rules.Add(subset.Concat(remaining).ToArray());
                            }
                        }
                    }
                }
            }
            ViewBag.r = rules;
            // عرض القواعد المكتشفة
          
            List<dynamic> rulesList = new List<dynamic>();
            foreach (var rule in rules)
            {
                string antecedent = string.Join(", ", rule.Take(rule.Length - 1));
                string consequent = rule.Last().ToString();
                double confidenceValue = (double)CountOccurences(data, rule) / CountOccurences(data, rule.Take(rule.Length - 1).ToArray());

                dynamic newRule = new System.Dynamic.ExpandoObject();
                newRule.Antecedent = antecedent;
                newRule.Consequent = consequent;
                newRule.ConfidenceValue = confidenceValue;
                rulesList.Add(newRule);
            }
            ViewBag.Rules = rulesList;
            return View();
        }
    

      
        private static int CountOccurences(List<int[]> data, int[] set)
        {
            int count = 0;

            foreach (var row in data)
            {
                if (set.All(s => row.Contains(s)))
                {
                    count++;
                }
            }

            return count;
        }

        private static List<int[]> Combinations(SortedSet<int> set, int size)
        {
            var result = new List<int[]>();

            if (size == 1)
            {
                foreach (var item in set)
                {
                    result.Add(new int[] { item });
                }
            }
            else if (set.Count == size)
            {
                result.Add(set.ToArray());
            }
            else
            {
                for (int i = 0; i <= set.Count - size; i++)
                {
                    var subset = new SortedSet<int>(set.Skip(i + 1).Take(size - 1));
                    foreach (var combination in Combinations(subset, size - 1))
                    {
                        var newCombination = new int[combination.Length + 1];
                        newCombination[0] = set.ElementAt(i);
                        Array.Copy(combination, 0, newCombination, 1, combination.Length);
                        result.Add(newCombination);
                    }
                }
            }

            return result;
        }
     
          

//عرض المشاهدات واضافة مشاهدات جديدة

        public IActionResult views()
        {
            var views = c.Views.ToList();
            ViewBag.v = views;
            return View();
        }

        [HttpPost]
        public IActionResult Add(Views model)
        {
            if (ModelState.IsValid)
            {
                // إنشاء كائن view جديد وتعيين القيم المدخلة في النموذج
                var view = new Views
                {
                    UserId = model.UserId,
                    MovieId = model.MovieId
                };

                // إضافة الكائن view الجديد إلى قاعدة البيانات
                c.Views.Add(view);
                c.SaveChanges();

                // إضافة البيانات الجديدة إلى ملف CSV
                string csvFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "data2.csv");
            
                using (var writer = new StreamWriter(csvFilePath, true))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    // كتابة القيم المدخلة مباشرة إلى ملف CSV
                    csv.WriteField(model.UserId);
                    csv.WriteField(model.MovieId);
                    csv.NextRecord(); // للانتقال إلى السطر التالي في ملف CSV
                }

                return RedirectToAction("views");
            }

            return View();
        }

    }

    }
   