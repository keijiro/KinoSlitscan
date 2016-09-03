Kino/Slitscan
=============

*Slitscan* is an image effect for Unity that simulates [slit-scan effect]
[Wikipedia] in real time.

![gif](http://67.media.tumblr.com/ac4871c151023e1297f505198dd3c654/tumblr_ocxowaWfD01qio469o1_320.gif)
![gif](http://67.media.tumblr.com/89ff943410b2ec25cab1f67120b9461a/tumblr_ocxowaWfD01qio469o2_320.gif)
![gif](http://66.media.tumblr.com/0f5b9b9a8134b941837c28f9d8e195fc/tumblr_ocxowaWfD01qio469o3_320.gif)

System Requirement
------------------

- Unity 5.4 or later

Performance
-----------

*Slitscan* is quite GPU intensive despite its simpleness. Especially, it spends
a lot of video memory to store frame history. For example, if the screen
resolution is set to 1920x1080, it allocates about 300 MiB of video memory.

Also it increases draw call count. It may affect CPU performance in some
situations.

License
-------

Copyright (C) 2016 Keijiro Takahashi

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

[Wikipedia]: https://en.wikipedia.org/wiki/Slit-scan_photography
