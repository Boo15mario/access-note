using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessNote;

internal sealed class AppletRegistry
{
    private readonly Dictionary<AppletId, IApplet> _appletsById;
    private readonly List<AppletDescriptor> _descriptorsByRegistrationOrder;

    public AppletRegistry(IEnumerable<IApplet> applets)
    {
        ArgumentNullException.ThrowIfNull(applets);

        _appletsById = new Dictionary<AppletId, IApplet>();
        _descriptorsByRegistrationOrder = new List<AppletDescriptor>();
        foreach (var applet in applets)
        {
            ArgumentNullException.ThrowIfNull(applet);

            if (!_appletsById.TryAdd(applet.Descriptor.Id, applet))
            {
                throw new InvalidOperationException($"Duplicate applet id registered: {applet.Descriptor.Id}");
            }

            _descriptorsByRegistrationOrder.Add(applet.Descriptor);
        }
    }

    public IApplet GetRequired(AppletId id)
    {
        if (_appletsById.TryGetValue(id, out var applet))
        {
            return applet;
        }

        throw new KeyNotFoundException($"No applet registered for id {id}");
    }

    public IReadOnlyList<AppletDescriptor> GetDescriptorsInRegistrationOrder()
    {
        return _descriptorsByRegistrationOrder;
    }

    public bool TryGetDescriptor(AppletId id, out AppletDescriptor descriptor)
    {
        if (_appletsById.TryGetValue(id, out var applet))
        {
            descriptor = applet.Descriptor;
            return true;
        }

        descriptor = null!;
        return false;
    }

    public AppletId? ResolveStartAppletId(StartScreenOption startScreen)
    {
        return _descriptorsByRegistrationOrder
            .FirstOrDefault(descriptor => descriptor.StartScreenOption == startScreen)
            ?.Id;
    }
}
