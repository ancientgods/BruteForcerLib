# BruteForcerLib
Selfwritten library for C# bruteforer, cracks various algorithms (e.g. SHA512)

Will generate passwords with the specified characaters, eg if the alphabet is being used it will start at a, then b, c, d, etc...
aa, ab, ac, ..., aaa, aab, ..., azz, zzzz...   You get the point

You can specify a minimum and maximum password length.
Has 3 events:

<b>OnPasswordFound</b> - will fire when the password has been found (obviously)<br>
<b>OnProgressChanged</b> - will fire in increments of 1% progress<br>
<b>OnFinish</b> - will fire when the bruteforcer has either found the password or finished the maximum number of possible combinations<br>


Example usage:
<a href=http://pastebin.com/u4gGs9Yh>Click Here</a>


<img src=http://puu.sh/fc4t9/0b6cf03b97.png></img>
