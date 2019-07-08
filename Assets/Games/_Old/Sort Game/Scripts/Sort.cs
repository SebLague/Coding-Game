using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sort : MonoBehaviour {
    int num = 10;

    void Start () {
        int[] values = new int[num];
        for (int i = 0; i < num; i++) {
            values[i] = Random.Range (1, 100);
        }

        int numSwaps = DoSort (values);
        print ("Num swaps: " + numSwaps);
        for (int i = 0; i < values.Length; i++) {
            print (values[i]);
        }
    }

    int DoSort (int[] values) {

        int numSwaps = 0;

        for (int numSorted = 0; numSorted < values.Length; numSorted++) {
            int numUnsorted = values.Length - numSorted;
            int largestUnsorted = 0;
            int largestUnsortedIndex = 0;

            for (int i = 0; i < numUnsorted; i++) {
                if (values[i] > largestUnsorted) {
                    largestUnsorted = values[i];
                    largestUnsortedIndex = i;
                }
            }
            int sortedIndex = numUnsorted - 1;
            if (largestUnsortedIndex != sortedIndex) {
                //Swap(largestUnsortedIndex,sortedIndex)
                print ("Swap " + largestUnsortedIndex + " : " + sortedIndex);
                numSwaps++;
                values[largestUnsortedIndex] = values[sortedIndex];
                values[sortedIndex] = largestUnsorted; // optional
            }
        }
        return numSwaps;
    }
}