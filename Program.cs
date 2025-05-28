using Microsoft.Playwright;
using Xunit;

namespace monkeytypeAuto;

public class UnitTest1
{
    [Fact]
    public async Task AutoTypeWordsRealistically()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync("https://monkeytype.com");

        // Accept cookies if visible
        var acceptBtn = page.Locator("div.main > div.buttons > button.acceptAll");
        if (await acceptBtn.IsVisibleAsync())
            await acceptBtn.ClickAsync();

        // Wait for the typing test container and focus the input field
        await page.WaitForSelectorAsync("div#typingTest > div#wordsWrapper");
        var input = page.Locator("input#wordsInput");
        await input.FocusAsync();

        // Set desired test duration or limit
        var testEndTime = DateTime.UtcNow.AddSeconds(30); // 15/30/60s based on Monkeytype test mode
        int wordsTyped = 0;
        var rnd = new Random();

        while (DateTime.UtcNow < testEndTime)
        {
            // Get the current active word
            var currentWordElement = await page.Locator("div.word.active").ElementHandleAsync();
            if (currentWordElement == null) break;

            var letterElements = await currentWordElement.QuerySelectorAllAsync("letter");
            string word = string.Join("", await Task.WhenAll(letterElements.Select(l => l.InnerTextAsync())));

            // Type the word one character at a time with realistic delay
            foreach (char c in word)
            {
                await page.Keyboard.InsertTextAsync(c.ToString());
                await Task.Delay(rnd.Next(35, 90)); // Simulate varying speed (approx. 80-130 WPM)
            }

            // Press space after each word
            await page.Keyboard.PressAsync(" ");
            await Task.Delay(rnd.Next(15, 40)); // Short pause after space

            wordsTyped++;
        }

        Console.WriteLine($"\n✅ Words typed: {wordsTyped}");

        await Task.Delay(5000); // Allow time to view score
        await browser.CloseAsync();
    }
}
