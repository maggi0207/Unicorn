import os

path = r'd:\Unicorn-repo\test\Test.UI.EmployerPortal.Web.Component\Pages\ManageAddressesTests.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

replacements = [
    ('() => _fakeSession.GetAsync<SelectedEmployerAccount>()', '() => { return _fakeSession.GetAsync<SelectedEmployerAccount>(); }'),
    ('() => _fakeService.GetAddressesAsync(A<int>._)', '() => { return _fakeService.GetAddressesAsync(A<int>._); }'),
    ('() => _fakeService.DeleteAddressAsync(A<long>._, A<int>._)', '() => { return _fakeService.DeleteAddressAsync(A<long>._, A<int>._); }'),
    ('() => Task.CompletedTask', '() => { return Task.CompletedTask; }'),
    ('b => b.TextContent.Trim() == \"CANCEL\"', 'b => { return b.TextContent.Trim() == \"CANCEL\"; }'),
    ('b => b.TextContent.Trim() == \"DELETE\"', 'b => { return b.TextContent.Trim() == \"DELETE\"; }'),
    ('() => addressTypeHeader.ClickAsync(new())', '() => { return addressTypeHeader.ClickAsync(new()); }'),
    ('() => headers[1].ClickAsync(new())', '() => { return headers[1].ClickAsync(new()); }'),
    ('() =>\r\n            cut.FindAll(\"button\").First(b => b.TextContent.Trim() == \"DELETE\").ClickAsync(new())', '() => { return cut.FindAll(\"button\").First(b => { return b.TextContent.Trim() == \"DELETE\"; }).ClickAsync(new()); }'),
    ('() =>\n            cut.FindAll(\"button\").First(b => b.TextContent.Trim() == \"DELETE\").ClickAsync(new())', '() => { return cut.FindAll(\"button\").First(b => { return b.TextContent.Trim() == \"DELETE\"; }).ClickAsync(new()); }')
]

for old, new in replacements:
    text = text.replace(old, new)

with open(path, 'w', encoding='utf-8') as f:
    f.write(text)

print('Successfully replaced all lambdas!')
