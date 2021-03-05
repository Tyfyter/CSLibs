namespace Tyfyter.Utils {
	public static class MiscUtils {
        public static T[] BuildArray<T>(int length, params int[] nonNullIndeces) where T : new() {
            T[] o = new T[length];
            for(int i = 0; i < nonNullIndeces.Length; i++) {
                if(nonNullIndeces[i] == -1) {
                    for(i = 0; i < o.Length; i++) {
                        o[i] = new T();
                    }
                    break;
                }
                o[nonNullIndeces[i]] = new T();
            }
            return o;
        }
	}
}