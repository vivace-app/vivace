using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtil {
	public static KeyCode GetKeyCodeByLineNum (int lineNum) {
		switch (lineNum) {
			case 0:
				return KeyCode.D;
			case 1:
				return KeyCode.F;
			case 2:
				return KeyCode.G;
			case 3:
				return KeyCode.H;
			case 4:
				return KeyCode.J;
			default:
				return KeyCode.None;
		}
	}
}