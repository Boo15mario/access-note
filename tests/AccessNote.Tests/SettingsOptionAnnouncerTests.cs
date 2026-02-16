using System.Collections.Generic;
using AccessNote;

namespace AccessNote.Tests;

public sealed class SettingsOptionAnnouncerTests
{
    [Fact]
    public void AnnounceSelectedOption_UpdatesHintAndAnnouncesMessage()
    {
        StaTestRunner.Run(
            () =>
            {
                var (adapter, _, _, _, optionHintText) = SettingsViewAdapterTestFactory.Create();
                var announced = new List<string>();
                var announcer = new SettingsOptionAnnouncer(adapter, announced.Add);
                var options = new List<SettingsOptionEntry>
                {
                    new()
                    {
                        Label = "Announce hints on screen open",
                        Hint = "Toggle introductory hints.",
                        GetValue = () => "On",
                        ChangeBy = _ => { },
                    },
                };

                announcer.AnnounceSelectedOption(options, optionIndex: 0);

                Assert.Equal("Toggle introductory hints.", optionHintText.Text);
                Assert.Equal(
                    "Announce hints on screen open. On. Toggle introductory hints.",
                    Assert.Single(announced));
            });
    }

    [Fact]
    public void AnnounceCategory_AndReadOnlyOption_UseExpectedMessages()
    {
        StaTestRunner.Run(
            () =>
            {
                var (adapter, _, _, _, _) = SettingsViewAdapterTestFactory.Create();
                var announced = new List<string>();
                var announcer = new SettingsOptionAnnouncer(adapter, announced.Add);

                announcer.AnnounceCategory("Advanced");
                announcer.AnnounceReadOnlyOption();

                Assert.Equal(
                    new[]
                    {
                        "Settings category. Advanced.",
                        "This option is read only.",
                    },
                    announced);
            });
    }
}
