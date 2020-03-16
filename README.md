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

