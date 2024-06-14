using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

string path = @"C:\Users\Professional\Downloads\SiM4_5min_05042024_05042024.txt";
string[] lines = File.ReadAllLines(path);
string[] dataLines = lines.Skip(1).ToArray();
var result = new OriginalData[dataLines.Length];
for (var i = 0; i < dataLines.Length; i++)
{
    string[] columns = dataLines[i].Split(';');
    result[i] = new OriginalData
    {
        tickers = columns[0],
        per = int.Parse(columns[1]),
        date = int.Parse(columns[2]),
        time = int.Parse(columns[3]),
        open = double.Parse(columns[4]),
        high = double.Parse(columns[5]),
        low = double.Parse(columns[6]),
        close = double.Parse(columns[7]),
        volume = double.Parse(columns[8]),
        openInt = double.Parse(columns[9]),

    };

}

var close = result.Select(x => x.close).ToArray();
var high = result.Select(h => h.high).ToArray();
var low = result.Select(l => l.low).ToArray();

var moderndata = new ModernData[dataLines.Length];
moderndata[0] = new ModernData
{
    Index = 0,
    Price = close[0]
};
for (var i = 1; i < dataLines.Length; i++)
{
    moderndata[i] = new ModernData
    {
        Index = i,
        Price = close[i],
        high = high[i],
        low = low[i],
        Differrence = Math.Log((double)(close[i] / close[i - 1])),


    };
}

for (var i = 0; i < dataLines.Length; i += 12)
{
    double?[] subsetLow = low.Skip(i).Take(12).ToArray();
    double? minValue = subsetLow.Min();
    double?[] subsetHigh = high.Skip(i).Take(12).ToArray();
    double? maxValue = subsetHigh.Max();

    // Находим размах колебаний для текущей группы данных
    double? amplitude = maxValue - minValue;

    // Определяем конечный индекс для текущей группы данных
    int endIndex = Math.Min(i + 12, dataLines.Length);

    // Устанавливаем значение размаха колебаний для каждого элемента в текущей группе
    for (int j = i; j < endIndex; j++)
    {
        moderndata[j].AmplitudeperHourly = amplitude;
    }
}

// Выводим значения AmplitudeperHourly на консоль
//foreach (var data in moderndata)
//{
//    Console.WriteLine($"AmplitudeperHourly: {data.AmplitudeperHourly}");
//}

var defference = moderndata.Select(m => m.Differrence).ToArray();

int groupCount = defference.Length / 12 + (defference.Length % 12 == 0 ? 0 : 1);
double[] averageDefference = new double[groupCount];
int dataIndex = 0; // Сброс индекса для перебора элементов defference
double[] volatilities = new double[groupCount]; // Массив для хранения волатильностей
for (int i = 0; i < groupCount; i++)
{
    // Вычисление начального индекса группы
    int startIndex = i * 12;

    // Вычисление конечного индекса группы
    int endIndex = Math.Min(startIndex + 12, defference.Length);

    // Вычисление среднего значения для текущей группы
    double sum = 0;
    int count = 0; // Количество элементов в текущей группе
    for (int j = startIndex; j < endIndex; j++)
    {
        sum += defference[j].GetValueOrDefault();
        count++;
    }
    double average = sum / count;

    // Установка разности и квадрата разности для каждого элемента в группе
    double sumDifferenceSqr = 0; // Сумма квадратов разностей для текущей группы
    for (int j = startIndex; j < endIndex; j++)
    {
        moderndata[dataIndex].Mathexpected = average;
        moderndata[dataIndex].Difference = defference[j].GetValueOrDefault() - average;
        moderndata[dataIndex].DifferenceSqr = moderndata[dataIndex].Difference * moderndata[dataIndex].Difference;
        sumDifferenceSqr += moderndata[dataIndex].DifferenceSqr;
        dataIndex++;
    }
    moderndata[i].SumDifferenceSqr = sumDifferenceSqr; // Запись суммы квадратов разностей для текущей группы

    // Расчет стандартного квадратного отклонения
    if (count > 1)
    {
        double standardDeviation = Math.Sqrt(sumDifferenceSqr / (count - 1)); // Формула для стандартного квадратного отклонения
        moderndata[i].StandardDeviation = standardDeviation;

        // Расчет волатильности
        double volatility = standardDeviation * Math.Sqrt(count); // Формула для волатильности
        volatilities[i] = volatility; // Сохраняем волатильность для текущей группы
    }
}

// Если остался остаток, вычисляем среднее для остаточных элементов
if (defference.Length % 12 != 0)
{
    int startIndex = groupCount * 12; // Начальный индекс для остаточных элементов
    double sum = 0;
    int count = 0; // Количество остаточных элементов
    for (int j = startIndex; j < defference.Length; j++)
    {
        sum += defference[j].GetValueOrDefault();
        count++;
    }
    double average = sum / count;

    // Установка разности и квадрата разности для каждого остаточного элемента
    double sumDifferenceSqr = 0; // Сумма квадратов разностей для остаточных элементов
    for (int j = startIndex; j < defference.Length; j++)
    {
        moderndata[dataIndex].Mathexpected = average;
        moderndata[dataIndex].Difference = defference[j].GetValueOrDefault() - average;
        moderndata[dataIndex].DifferenceSqr = moderndata[dataIndex].Difference * moderndata[dataIndex].Difference;
        sumDifferenceSqr += moderndata[dataIndex].DifferenceSqr;
        dataIndex++;
    }
    moderndata[groupCount - 1].SumDifferenceSqr = sumDifferenceSqr; // Запись суммы квадратов разностей для остаточных элементов

    // Расчет стандартного квадратного отклонения для остаточных элементов
    if (count > 1)
    {
        double standardDeviation = Math.Sqrt(sumDifferenceSqr / (count - 1)); // Формула для стандартного квадратного отклонения
        moderndata[groupCount - 1].StandardDeviation = standardDeviation;

        // Расчет волатильности для остаточных элементов
        double volatility = standardDeviation * Math.Sqrt(count); // Формула для волатильности
        volatilities[groupCount - 1] = volatility; // Сохраняем волатильность для остаточных элементов
    }
}

// Установка волатильности для каждой группы
for (int i = 0; i < groupCount; i++)
{
    moderndata[i].Volatility = volatilities[i];
   
}


foreach (var data in moderndata)
{
    Console.WriteLine($"Амплитуда {data.AmplitudeperHourly.},волатильност - {data.Volatility * 100}");  

}



Console.ReadLine();
public class ModernData
{
    public int Index { get; set; }
    public double? Price { get; set; } = null!;
    public double? high { get; set; } = null!;
    public double? low { get; set; } = null!;

    public double? AmplitudeperHourly  { get; set; } = null; //часовая амплитуда high - low
    public double? Differrence { get; set; } = null!;
    public double? Mathexpected { get; set; } = null!;
    public double Difference { get; set; } 
    public double DifferenceSqr { get; set; } 
    public double? SumDifferenceSqr { get; set; } = null!;
    public double? StandardDeviation { get; set; } = null!;
    public double? Volatility { get; set; } = null!;

    public override string ToString() => $"Index {Index}, Price {Price},High {high},Low {low},AmplitudeperHourly  {AmplitudeperHourly}, Differrence: {Differrence}," +
        $",Mathexpected: {Mathexpected}," + $"Difference: {Difference}, DifferenceSqr:{DifferenceSqr}, SumDifferenceSqr: {SumDifferenceSqr}, STDSumDeviationSqar: {SumDifferenceSqr}," +
           $"STDSumDeviationSqar: {StandardDeviation},Volatilyty: {Volatility}";

}

public class OriginalData
{
    public string? tickers { get; set; }
    public int per { get; set; }
    public int date { get; set; }
    public int time { get; set; }
    public double? open { get; set; } = null!;
    public double? high { get; set; } = null!;
    public double? low { get; set; } = null!;
    public double? close { get; set; } = null!;
    public double? previousClose { get; set; } = null!;
    public double? volume { get; set; } = null!;
    public double? openInt { get; set; } = null!;


    public override string ToString() => $" <TICKER> {tickers}, <PER> {per},<DATE> {date},<TIME>" +
        $" {time},<OPEN> {open},<HIGH> {high},<LOW> {low},CLOSE {close}, VOL{volume},OPENINT {openInt}";

}