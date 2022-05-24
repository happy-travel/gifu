# Gifu.
Virtual credit card issuer

### Summary
The project issues virtual credit cards for suppliers

[American Express API documentation](https://developer.americanexpress.com)

### Infrastructure Dependencies
* Database connection string
* Sentry endpoint
* Access to Vault

### Project dependencies
No dependencies

### Examples
#### Request
POST /api/1.0/cards
```json
{
	"referenceCode": "HTL-EA-00001-01",
	"moneyAmount": {
		"currency": "USD",
		"amount": 150
	},
	"dueDate": "2022-01-01"
}
```
#### Response
```json
{
	"number": "1111222233334444",
	"expiry": "2020-10-10T00:00:00:00",
	"holder": "Cardholder Name",
	"code": "123"
}
```
