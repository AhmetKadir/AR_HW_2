# AR_HW_2

![image](https://github.com/AhmetKadir/AR_HW_2/assets/72475558/0007efac-0ad0-4bde-a4b6-a5ccf6851f76)

Source Points are yellow.

Target Points are blue.

Transformated points (From Source to Target) are gray.


First, get (n,3) combinations from both points set.

Then, calculate RT between them.

Then, apply this RT to all of source points. We get a set of points here, calculate the distance between these set of points and target points.

If error (mean distance) is smaller than 0.1, accept these points as transformedPoints and finish.

If error is smaller than the best case (bestMeanError, which I started from 1000) assign error to bestMeanError, accept these points as transformedPoints and try another set of combination.


So, loop will finish if we find mean distance smaller than 0.1 or if we try all combinations (we will accept the one with smallest distance here).
