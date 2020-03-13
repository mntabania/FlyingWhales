using UnityEngine;
using System.Collections;
using System.Linq;

public struct GameDate {
	public int month;
	public int day;
	public int year;
    public int tick;

    public GameDate(int month, int day, int year, int tick){
        this.month = month;
		this.day = day;
		this.year = year;
        this.tick = tick;
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

    public string ConvertToContinuousDaysWithTime(bool nextLineTime = false) {
        if (nextLineTime) {
            return $"Day {ConvertToContinuousDays()}\n{GameManager.ConvertTickToTime(this.tick)}";
        }
        return $"Day {ConvertToContinuousDays()} {GameManager.ConvertTickToTime(this.tick)}";
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
