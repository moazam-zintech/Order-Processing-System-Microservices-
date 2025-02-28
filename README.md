
## Technologies Used

*   **Language:** C# (.NET 7 or later)
*   **Framework:** ASP.NET Core Web API
*   **Message Broker:** RabbitMQ
*   **Database:** SQLite (for demonstration, easily replaced with PostgreSQL, SQL Server, etc.)
*   **API Gateway:** YARP (Yet Another Reverse Proxy)
*   **Serialization:** Newtonsoft.Json
*   **Logging:** Microsoft.Extensions.Logging

## Prerequisites

*   .NET 7 SDK or later
*   RabbitMQ Server (installed and running)
*   Visual Studio or a compatible IDE (e.g., VS Code with C# extension)

## Setup Instructions

1.  **Clone the Repository (if applicable)**

    ```bash
    git clone <repository_url>
    cd <project_directory>
    ```

2.  **Create ASP.NET Core Web API Projects:**

    If you don't have existing projects, create them using the .NET CLI or Visual Studio:

    ```bash
    dotnet new webapi -n OrderService
    dotnet new webapi -n PaymentService
    dotnet new webapi -n NotificationService
    dotnet new webapi -n ApiGateway
    dotnet new classlib -n OrderProcessing.Shared
    ```

3.  **Install NuGet Packages:**

    In each project (OrderService, PaymentService, NotificationService, ApiGateway), install the `RabbitMQ.Client` NuGet package. You'll also need other packages, depending on your chosen database:

    ```bash
    # Example (using .NET CLI):
    cd OrderService
    dotnet add package RabbitMQ.Client
    dotnet add package Microsoft.EntityFrameworkCore.Sqlite  # If using SQLite
    dotnet add package Microsoft.EntityFrameworkCore.Design   # Required for migrations
    dotnet tool install --global dotnet-ef #Install ef tool
    cd .. #go to the solution level
    dotnet ef migrations add InitialCreate -p OrderService -s OrderService
    dotnet ef database update -p OrderService -s OrderService

    cd PaymentService
    dotnet add package RabbitMQ.Client

    cd NotificationService
    dotnet add package RabbitMQ.Client

    cd ApiGateway
    dotnet add package Yarp.ReverseProxy
    ```

4.  **Configure RabbitMQ:**

    *   Ensure RabbitMQ is running on your system.
    *   Modify the `RabbitMqConfig` class in `OrderProcessing.Shared` and the `appsettings.json` files in each service to match your RabbitMQ server settings (hostname, etc.). By default, it uses `localhost`.
    *   Make sure the exchanges and queues defined in `RabbitMqConfig` exist in your RabbitMQ server. You can create them using the RabbitMQ management UI (usually accessible at `http://localhost:15672/`).

5.  **Database Configuration (Order Service):**

    *   If using SQLite, configure the connection string in the `appsettings.json` of the Order Service:

        ```json
        {
          "ConnectionStrings": {
            "OrderDatabase": "Data Source=orders.db"
          }
        }
        ```
    * Ensure you run the database migrations:
        ```bash
        cd OrderService
        dotnet ef database update
        ```

6.  **API Gateway Configuration:**

    *   In the `ApiGateway` project, configure the YARP reverse proxy routes and clusters in `appsettings.json`.  Adjust the service addresses to match the ports your services will be running on.  Example:

        ```json
        {
          "ReverseProxy": {
            "Routes": {
              "orders": {
                "ClusterId": "ordersCluster",
                "Match": {
                  "Path": "api/orders/{**catch-all}"
                }
              },
              "payments": {
                "ClusterId": "paymentsCluster",
                "Match": {
                  "Path": "api/payments/{**catch-all}"
                }
              },
              "notifications": {
                "ClusterId": "notificationsCluster",
                "Match": {
                  "Path": "api/notifications/{**catch-all}"
                }
              }
            },
            "Clusters": {
              "ordersCluster": {
                "Destinations": {
                  "ordersService": {
                    "Address": "https://localhost:7001/"
                  }
                }
              },
              "paymentsCluster": {
                "Destinations": {
                  "paymentsService": {
                    "Address": "https://localhost:7002/"
                  }
                }
              },
              "notificationsCluster": {
                "Destinations": {
                  "notificationsService": {
                    "Address": "https://localhost:7003/"
                  }
                }
              }
            }
          }
        }
        ```

7.  **Update the code to match the provided implementation.**

8.  **Run the Applications:**

    Run each service (OrderService, PaymentService, NotificationService) and the ApiGateway.

## Usage

1.  **Accessing the Services through the API Gateway:**

    All requests should be routed through the API Gateway.  Here are some example requests (assuming the API Gateway is running on `https://localhost:5000`):

    *   **Create Order:** `POST https://localhost:5000/api/orders`
        ```json
        {
          "customerId": "123",
          "totalAmount": 100.00
        }
        ```
    *   **Get Order:** `GET https://localhost:5000/api/orders/{orderId}`
    *   **Process Payment:** `POST https://localhost:5000/api/payments/process/{orderId}`

## RabbitMQ Message Flow

1.  **Order Creation:**
    *   The Order Service receives a request to create an order.
    *   The Order Service creates the order in its database and publishes an "Order Created" message to the `order_exchange`.

2.  **Payment Processing:**
    *   The Payment Service's `OrderConsumer` subscribes to the `order_payment_queue` (bound to `order_exchange`) and receives the "Order Created" message.
    *   The Payment Service simulates payment processing (either succeeds or fails randomly).
    *   The Payment Service publishes a `PaymentResult` message to the `payment_exchange`.

3.  **Notification:**
    *   The Notification Service's `PaymentResultConsumer` subscribes to the `payment_notification_queue` (bound to `payment_exchange`) and receives the `PaymentResult` message.
    *   The Notification Service sends an email/SMS notification to the customer based on the payment result.

4.  **Order Status Update:**
    *   The Order Service's `PaymentResultConsumer` subscribes to the `order_update_queue` (bound to `payment_exchange`) and receives the `PaymentResult` message.
    *   The Order Service updates the order status in its database based on the `PaymentResult`.

## Enhancements

*   **Shipping Service:** Add a Shipping Service to handle order fulfillment and publish "Order Shipped" messages.
*   **Retry Mechanisms:** Implement retry mechanisms for failed payment processing and other operations.
*   **Centralized Logging:** Add logging to all services and publish the logs to a central logging service (e.g., ELK Stack, Seq).
*   **Monitoring:** Implement health checks and monitoring for all services and RabbitMQ.
*   **Security:** Secure the API Gateway, RabbitMQ connections, and inter-service communication.
*   **Error Handling and Idempotency:** Improve error handling and ensure idempotency of message handlers.
*   **Dockerize the Application:** Package each service and the API Gateway into Docker containers for easier deployment and scaling.

## Troubleshooting

*   **RabbitMQ Connection Issues:** Check that the RabbitMQ server is running and that the connection settings in `appsettings.json` are correct.
*   **Message Consumption Errors:** Verify that the queues are correctly bound to the exchanges and that the message format is valid.
*   **API Gateway Routing Problems:** Inspect the YARP configuration in `appsettings.json` and ensure that the routes and cluster destinations are correctly configured.

## License

[Choose a license, e.g., MIT License]
