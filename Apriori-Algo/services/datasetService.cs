using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Apriori_Algo.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
namespace Apriori_Algo.services
{
    public class datasetService
    {
        private readonly MyDbContext _context;

        public datasetService(MyDbContext context)
        {
            _context = context;
        }

        public async Task InsertUserMoviesAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // قراءة البيانات من الملف CSV وتحويلها إلى قائمة من الكائنات UserMovie
            var records = csv.GetRecords<Views>().ToList();

            // إضافة البيانات إلى قاعدة البيانات باستخدام DbContext
            await _context.Views.AddRangeAsync(records);
            await _context.SaveChangesAsync();
        }
    }
}
