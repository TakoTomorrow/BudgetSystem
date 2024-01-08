using System.Globalization;

namespace BudgetSystem;

public class BudgetService
{
    private IBudgetRepo _budgetRepo;

    public BudgetService(IBudgetRepo budgetRepo)
    {
        _budgetRepo = budgetRepo;
    }

    public decimal Query(DateTime start, DateTime end)
    {

        if (start > end)
            return 0.0m;

        var data = _budgetRepo.GetAll().Select(s =>
        {
            var year = int.Parse(s.YearMonth.Substring(0, 4));
            var month = int.Parse(s.YearMonth.Substring(4, 2));
            var days = DateTime.DaysInMonth(year, month);
            return new
            {
                Year = year,
                Month = month,
                DailyAmount = s.Amount / days
            };
        });

        List<DateResult> dayRange = new List<DateResult>();

        if (start.Year == end.Year && start.Month == end.Month)
            dayRange.Add(new DateResult()
            {
                Year = start.Year,
                Month = start.Month,
                Days = end.Day - start.Day + 1
            });
        else
        {
            var startDays = DateTime.DaysInMonth(start.Year, start.Month);

            dayRange.Add(new DateResult()
            {
                Year = start.Year,
                Month = start.Month,
                Days = startDays - start.Day + 1
            });

            dayRange.Add(new DateResult()
            {
                Year = end.Year,
                Month = end.Month,
                Days = end.Day
            });

            var secondMonth = start.AddMonths(1);

            for (var date = secondMonth; date < end; date.AddMonths(1))
            {
                if (date.Year == end.Year && date.Month == end.Month)
                    break;

                dayRange.Add(new DateResult()
                {
                    Year = date.Year,
                    Month = date.Month,
                    Days = DateTime.DaysInMonth(date.Year, date.Month)
                });

            }
        }


        return dayRange.Join(data,
        day => new { day.Year, day.Month },
        dd => new { dd.Year, dd.Month },
        (day, dd) => dd.DailyAmount * day.Days
        ).Sum();
    }
}

public interface IBudgetRepo
{
    List<Budget> GetAll();
}

public class Budget
{
    public string YearMonth { get; set; }

    public int Amount { get; set; }
}

public class DateResult
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Days { get; set; }
}