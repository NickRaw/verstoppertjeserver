# Verstoppertje server Socket Connector

Deze connector zorgt ervoor dat je als client kunt verbinden met de verstoppertje server

## Versie 1.0.5
- Testconnect is ingebouwd en startsocket.
- Responses worden in een lijst gezet die door de responselistener() opgehaald worden.
- Responses zijn in json. Dus omzetten moet naar JObject.

## Gebruik
- Importeer ClientSocket
- ISocket gebruik je voor de socket
- ISocket.StartSocket heeft het ipaddress in string vorm en portnummer in integer vorm nodig voor verbinding. Start deze als eerste voordat je andere berichten stuurt.