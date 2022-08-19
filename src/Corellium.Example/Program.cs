using Corellium.Api;

var corellium = new CorelliumClient(new CorelliumOptions(
    endpoint: "https://app.corellium.com",
    apiToken: ""
));

await corellium.GetAccessTokenAsync();
await corellium.GetAccessTokenAsync();
