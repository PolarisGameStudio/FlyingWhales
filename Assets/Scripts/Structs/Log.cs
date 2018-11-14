﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Log {

    public int id;

	public MONTH month;
	public int day;
	public int year;
    public int hour;

	public string category;
	public string file;
	public string key;

	public List<LogFiller> fillers;

    public string logCallStack;

    public Log(int month, int day, int year, int hour, string category, string file, string key){
        this.id = Utilities.SetID<Log>(this);
		this.month = (MONTH)month;
		this.day = day;
		this.year = year;
        this.hour = hour;
		this.category = category;
		this.file = file;
		this.key = key;
		this.fillers = new List<LogFiller>();
        logCallStack = StackTraceUtility.ExtractStackTrace();
	}
    public Log(GameDate date, string category, string file, string key) {
        this.id = Utilities.SetID<Log>(this);
        this.month = (MONTH)date.month;
        this.day = date.day;
        this.year = date.year;
        this.hour = date.hour;
        this.category = category;
        this.file = file;
        this.key = key;
        this.fillers = new List<LogFiller>();
        logCallStack = StackTraceUtility.ExtractStackTrace();
    }

    internal void AddToFillers(object obj, string value, LOG_IDENTIFIER identifier){
		this.fillers.Add (new LogFiller (obj, value, identifier));
	}
    public void SetFillers(List<LogFiller> fillers) {
        this.fillers = fillers;
    }
    public void AddLogToInvolvedObjects() {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            object obj = currFiller.obj;
            if (obj != null) {
                if (obj is ICharacter) {
                    (obj as ICharacter).AddHistory(this);
                } else if (obj is BaseLandmark) {
                    (obj as BaseLandmark).AddHistory(this);
                } else if (obj is Minion) {
                    (obj as Minion).icharacter.AddHistory(this);
                }
            }
        }
    }
    public bool HasFillerForIdentifier(LOG_IDENTIFIER identifier) {
        for (int i = 0; i < fillers.Count; i++) {
            LogFiller currFiller = fillers[i];
            if (currFiller.identifier == identifier) {
                return true;
            }
        }
        return false;
    }
}
