﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Actionables {
	public class PlayerAction {
		
		public string actionName { get; private set; }
        public string labelText { get; private set; }
		public System.Func<bool> isActionValidChecker { get; private set; }
		public List<System.Action> actions { get; private set; }

        public ActionItem actionItem { get; private set; }

		public PlayerAction(string _name, System.Func<bool> _isActionValidChecker, params System.Action[] _actions) {
			actionName = _name;
            labelText = actionName;
			isActionValidChecker = _isActionValidChecker;
			actions = _actions?.ToList() ?? null;
		}
        public void SetLabelText(string text) {
            labelText = text;
        }
        public void SetActionItem(ActionItem item) {
            actionItem = item;
        }
		public void Execute() {
			if (actions != null) {
				for (int i = 0; i < actions.Count; i++) {
					System.Action currentAction = actions[i];
					currentAction.Invoke();
				}
			}
			Messenger.Broadcast(Signals.PLAYER_ACTION_EXECUTED, this);
		}
		
	}	
}
