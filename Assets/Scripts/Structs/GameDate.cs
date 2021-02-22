using System;
using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public struct GameDate {
	public int month;
	public int day;
	public int year;
    public int tick;
    public bool hasValue;
    
    public GameDate(int month, int day, int year, int tick){
        this.month = month;
		this.day = day;
		this.year = year;
        this.tick = tick;
        hasValue = true;
    }

    public GameDate AddTicks(int amount) {
        this.tick += amount;
        while (this.tick > GameManager.ticksPerDay) {
            this.tick -= GameManager.ticksPerDay;
            AddDays(1);
        }
        return this;
    }

	public GameDate AddDays(int amount){
		this.day += amount;
        int count = 0;
		while (this.day > GameManager.daysPerMonth) {
			this.day -= GameManager.daysPerMonth;
            count++;
		}
        if(count > 0) {
            AddMonths(count);
        }
        return this;
    }
	public void AddMonths(int amount){
		this.month += amount;
		while (this.month > 12) {
			this.month -= 12;
            AddYears(1);
        }
        if(this.day > GameManager.daysPerMonth) {
            this.day = GameManager.daysPerMonth;
        }
	}
	public void AddYears(int amount){
		this.year += amount;
	}

    public void ReduceTicks(int amount) {
        for (int i = 0; i < amount; i++) {
            this.tick -= 1;
            if (this.tick <= 0) {
                ReduceDays(1);
                this.tick = GameManager.ticksPerDay;
            }
        }
    }
    public void ReduceDays(int amount) {
        for (int i = 0; i < amount; i++) {
            this.day -= 1;
            if (this.day == 0) {
                ReduceMonth(1);
                this.day = GameManager.daysPerMonth;
            }
        }
    }
    public void ReduceMonth(int amount) {
        for (int i = 0; i < amount; i++) {
            this.month -= 1;
            if (this.month == 0) {
                this.month = 12; //last month
                ReduceYear(1);
            }
        }
    }
    public void ReduceYear(int amount) {
        for (int i = 0; i < amount; i++) {
            this.year -= 1;
        }
    }
	public void SetDate(int month, int day, int year, int tick){
		this.month = month;
		this.day = day;
		this.year = year;
        this.tick = tick;
	}
    public void SetTicks(int tick) {
        this.tick = tick;
    }
	public void SetDate(GameDate gameDate){
		this.month = gameDate.month;
		this.day = gameDate.day;
		this.year = gameDate.year;
        this.tick = gameDate.tick;
	}
	public bool IsSameDate(int month, int day, int year, int tick){
		if(this.month == month && this.day == day && this.year == year && this.tick == tick){
			return true;
		}
		return false;
	}
	public bool IsSameDate(GameDate gameDate){
		if(this.month == gameDate.month && this.day == gameDate.day && this.year == gameDate.year && this.tick == gameDate.tick) {
			return true;
		}
		return false;
	}

    /*
     Is this date before other date
         */
    public bool IsBefore(GameDate otherDate) {
        if (this.year < otherDate.year) {
            return true;
        } else if (this.year == otherDate.year) {
            //the 2 dates are of the same year
            if (this.month < otherDate.month) {
                //this.month is less than the otherDate.month
                return true;
            } else if (this.month == otherDate.month) {
                //this.month is equal to otherDate.month
                if (this.day < otherDate.day) {
                    return true;
                } else if (this.day == otherDate.day) {
                    if (this.tick < otherDate.tick) {
                        return true;
                    } else if (this.tick == otherDate.tick) {
                        //the 2 dates are the exact same, return false
                        return false;
                    } else {
                        return false;
                    }
                } else {
                    //this.day is greater than otherDate.year
                    return false;
                }
            } else {
                //this.month is greater than otherDate.month)
                return false;
            }
        } else {
            //this.year is greater than otherDate.year
            return false;
        }
       
    }

    /*
     Is this date after other date
         */
    public bool IsAfter(GameDate otherDate) {
        if (this.year < otherDate.year) {
            return false;
        } else if (this.year == otherDate.year) {
            //the 2 dates are of the same year
            if (this.month < otherDate.month) {
                //this.month is less than the otherDate.month
                return false;
            } else if (this.month == otherDate.month) {
                //this.month is equal to otherDate.month
                if (this.day < otherDate.day) {
                    return false;
                } else if (this.day == otherDate.day) {
                    if (this.tick < otherDate.tick) {
                        return false;
                    } else if (this.tick == otherDate.tick) {
                        //the 2 dates are the exact same, return false
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    //this.day is greater than otherDate.day
                    return true;
                }
            } else {
                //this.month is greater than otherDate.month)
                return false;
            }
        } else {
            //this.year is greater than otherDate.year
            return true;
        }

    }

    public string ToStringDate(){
		return $"{((MONTH) this.month).ToString()} {this.day.ToString()}, {this.year.ToString()} T: {this.tick.ToString()}";
	}
    public override string ToString() {
        return ConvertToContinuousDaysWithTime();
    }
    public int ConvertToContinuousDays() {
        int totalDays = 0;
        if (year > GameManager.Instance.startYear) {
            int difference = year - GameManager.Instance.startYear;
            totalDays += ((difference * 12) * GameManager.daysPerMonth);
        }
        totalDays += (((month - 1) * GameManager.daysPerMonth) + day);
        return totalDays;
    }

    public int Sum() {
        return ConvertToContinuousDays() + tick;
    }
    /// <summary>
    /// Get the number of hours between this date and another date.
    /// NOTE: This does not return a negative value, so this doesn't
    /// take into account which date is earlier and which is later.
    /// </summary>
    /// <param name="otherDate">The date to compare to.</param>
    /// <returns>The number of hours between the 2 dates.</returns>
    public int GetHourDifference(GameDate otherDate) {
        int yearDifference = Math.Abs(year - otherDate.year);
        int monthDifference = Math.Abs(month - otherDate.month);
        int tickDifference = Math.Abs(tick - otherDate.tick);

        //difference in years multiplied by (number of ticks per day * number of days in a year)
        int yearDifferenceInTicks = yearDifference * (GameManager.ticksPerDay * 360);
        //difference in months multiplied by (number of ticks per day * number of days per month)
        int monthDifferenceInTicks = monthDifference * (GameManager.ticksPerDay * 30);
        int totalTickDifference = yearDifferenceInTicks + monthDifferenceInTicks + tickDifference;
        return GameManager.Instance.GetHoursBasedOnTicks(totalTickDifference);
    }
    /// <summary>
    /// Get the number of ticks between this date and another date.
    /// NOTE: This does not return a negative value, so this doesn't
    /// take into account which date is earlier and which is later.
    /// otherDate should be the earlier date.
    /// </summary>
    /// <param name="otherDate">The date to compare to.</param>
    /// <returns>The number of ticks between the 2 dates.</returns>
    public int GetTickDifference(GameDate otherDate) {
        int yearDifference = Math.Abs(year - otherDate.year);
        int monthDifference = Math.Abs(month - otherDate.month);
        int dayDifference = Math.Abs(day - otherDate.day);
        int tickDifference = Math.Abs(tick - otherDate.tick);

        //difference in years multiplied by (number of ticks per day * number of days in a year)
        int yearDifferenceInTicks = yearDifference * (GameManager.ticksPerDay * 360);
        //difference in months multiplied by (number of ticks per day * number of days per month)
        int monthDifferenceInTicks = monthDifference * (GameManager.ticksPerDay * 30);
        //difference in days multiplied by number of ticks per day
        int dayDifferenceInTicks = dayDifference * GameManager.ticksPerDay;
        
        int totalTickDifference = yearDifferenceInTicks + monthDifferenceInTicks;
        if (dayDifference > 0) {
            if (tick < otherDate.tick) {
                totalTickDifference += dayDifferenceInTicks + tickDifference;
            } else {
                totalTickDifference += dayDifferenceInTicks - tickDifference;
            }
        } else {
            totalTickDifference += tickDifference;
        }
        return totalTickDifference;
    }

    public int GetTickDifferenceNonAbsoluteOrZeroIfReached(GameDate otherDate) {
        int yearDifference = (otherDate.year - year);
        int monthDifference = (otherDate.month - month);
        int dayDifference = (otherDate.day - day);
        int tickDifference = Mathf.Abs(otherDate.tick - tick);

        //difference in years multiplied by (number of ticks per day * number of days in a year)
        int yearDifferenceInTicks = yearDifference * (GameManager.ticksPerDay * 360);
        //difference in months multiplied by (number of ticks per day * number of days per month)
        int monthDifferenceInTicks = monthDifference * (GameManager.ticksPerDay * 30);
        //difference in days multiplied by number of ticks per day
        int dayDifferenceInTicks = dayDifference * GameManager.ticksPerDay;

        if (dayDifference <= -1) {
            return 0;
        }
        if (dayDifference <= 0) {
            if (tick >= otherDate.tick) {
                return 0;
            }
        }
        
        int totalTickDifference = yearDifferenceInTicks + monthDifferenceInTicks;
        if (dayDifference > 0) {
            if (tick < otherDate.tick) {
                totalTickDifference += dayDifferenceInTicks + tickDifference;
            } else {
                totalTickDifference += dayDifferenceInTicks - tickDifference;
            }
        } else {
            totalTickDifference += tickDifference;
        }
        return totalTickDifference;
    }

    public string GetTimeDifferenceString(GameDate otherDate) {
        int tickDiff = GetTickDifference(otherDate);
        if (tickDiff >= GameManager.ticksPerHour) {
            int hours = GameManager.Instance.GetHoursBasedOnTicks(tickDiff);
            if (hours > 1) {
                return $"{hours.ToString()} hours";
            } else {
                return $"{hours.ToString()} hour";
            }
        } else {
            int minutes = GameManager.Instance.GetMinutesBasedOnTicks(tickDiff);
            if (minutes > 1) {
                return $"{minutes.ToString()} minutes";    
            } else {
                return $"{minutes.ToString()} minute";
            }
                        
        }
    }
    
    public string ConvertToContinuousDaysWithTime(bool nextLineTime = false) {
        if (!hasValue) {
            return "???";
        }
        if (nextLineTime) {
            return $"Day {ConvertToContinuousDays()}\n{ConvertToTime()}";
        }
        return $"Day {ConvertToContinuousDays()} {ConvertToTime()}";
    }
    public string ConvertToTime() {
        return $"{GameManager.Instance.ConvertTickToTime(tick)}";
    }

    //public override bool Equals(object obj) {
    //    //if (obj is GameDate) {
    //    //    return Equals((GameDate)obj);
    //    //}
    //    return base.Equals(obj);
    //}

    //public bool Equals(GameDate otherDate) {
    //    if (this.year == otherDate.year && this.month == otherDate.month && this.day == otherDate.day) {
    //        return true;
    //    }
    //    return false;
    //}
}
