import os

path = r'd:\Unicorn-repo\test\Test.UI.EmployerPortal.Web.Component\Pages\ManageAddressesTests.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

# Revert A.CallTo lambdas
text = text.replace('A.CallTo(() => { return _fakeSession.GetAsync<SelectedEmployerAccount>(); })', 'A.CallTo(() => _fakeSession.GetAsync<SelectedEmployerAccount>())')
text = text.replace('A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })', 'A.CallTo(() => _fakeService.GetAddressesAsync(A<int>._))')
text = text.replace('A.CallTo(() => { return _fakeService.DeleteAddressAsync(A<long>._, A<int>._); })', 'A.CallTo(() => _fakeService.DeleteAddressAsync(A<long>._, A<int>._))')
text = text.replace('A.CallTo(() => { return _fakeService.DeleteAddressAsync(2L, A<int>._); })', 'A.CallTo(() => _fakeService.DeleteAddressAsync(2L, A<int>._))')

# Fix IDE0022 for MakeRow
make_row_old = '''    private static AddressRowModel MakeRow(
        long sk = 1,
        string type = "Main Physical Location",
        string address = "123 Main St, Madison, WI 53701",
        bool canDelete = true,
        int addressTypeCodeSK = 13) => new()
        {
            AddressSK = sk,
            AddressTypeCodeSK = addressTypeCodeSK,
            AddressType = type,
            FormattedAddress = address,
            CanDelete = canDelete,
            CountryAddressFormatCodeSK = 1
        };'''

make_row_new = '''    private static AddressRowModel MakeRow(
        long sk = 1,
        string type = "Main Physical Location",
        string address = "123 Main St, Madison, WI 53701",
        bool canDelete = true,
        int addressTypeCodeSK = 13)
    {
        return new()
        {
            AddressSK = sk,
            AddressTypeCodeSK = addressTypeCodeSK,
            AddressType = type,
            FormattedAddress = address,
            CanDelete = canDelete,
            CountryAddressFormatCodeSK = 1
        };
    }'''
text = text.replace(make_row_old, make_row_new)

# Fix IDE0022 for MainMailingRow
main_mailing_old = '''    private static AddressRowModel MainMailingRow() =>
        MakeRow(sk: 1, type: "Main Business Mailing Address", canDelete: false, addressTypeCodeSK: 11);'''

main_mailing_new = '''    private static AddressRowModel MainMailingRow()
    {
        return MakeRow(sk: 1, type: "Main Business Mailing Address", canDelete: false, addressTypeCodeSK: 11);
    }'''
text = text.replace(main_mailing_old, main_mailing_new)


with open(path, 'w', encoding='utf-8') as f:
    f.write(text)

print('Success!')
