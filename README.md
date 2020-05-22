# UIForia User Guide
Please head over to our [online documentation](https://uiforia.io/docs)!

## Quick Guide to local Unity Package

1. Install npm
2. Install verdaccio: `$ npm install --global verdaccio`
3. Run verdaccio: `$ verdaccio`
4. `cd Packages\UIForia`
5. Publish: `npm publish --registry http://localhost:4873`

Now add this to your project's `Pagackes/manifest.json`

```
{
  "scopedRegistries": [
    {
      "name": "Local registry",
      "url": "http://localhost:4873",
      "scopes": [
        "com.klang-games"
      ]
    }
  ],
  "dependencies": {
    "com.klang-games.uiforia": "0.1.0",
```

# Contribute
If you find a bug or have a cool feature idea please file a ticket, open a pull request or just
talk to us!


# License - MIT

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
