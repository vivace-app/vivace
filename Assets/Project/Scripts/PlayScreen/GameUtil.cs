using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtil {

	public static KeyCode GetKeyCodeByLineNum (int lineNum) { //Self Containmen
		switch (lineNum) { //キーコード割当
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