﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DateRange {

    public GameDate startDate;
    public GameDate endDate;

    //public int rangeInTicks { get { return GetRangeInTicks(); } }

    public DateRange(GameDate startDate, GameDate endDate) {
        this.startDate = startDate;
        this.endDate = endDate;
    }

    public bool IsDateInRange(GameDate date) {
        if (date.IsSameDate(startDate) || date.IsSameDate(endDate)) {
            return true; //date is the same as start or end date
        } else if (date.IsAfter(startDate) && date.IsBefore(endDate)) {
            return true; //date is in between start and end date
        } else {
            return false; //date is not in range
        }
    }

    public bool HasConflictWith(DateRange other) {
        if (IsDateInRange(other.startDate) || IsDateInRange(other.endDate)) {
            //if the start or end date of the other DateRange is inside this DateRange, they have conflicts (Intersecting dates)
            return true;
        }
        return false;
    }

    public override string ToString() {
        return startDate.GetDayAndTicksString() + " - " + endDate.GetDayAndTicksString();
    }

    public int GetStartDateValue() {
        return (startDate.day * GameManager.hoursPerDay)  + startDate.hour;
    }

    //private int GetRangeInTicks() {
    //    GameDate startDate = this.startDate;
    //    GameDate endDate = this.endDate;

    //}
}
