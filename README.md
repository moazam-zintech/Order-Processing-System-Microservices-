# Order Processing System (Microservices)

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-7.0-blueviolet)](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.9.0-orange)](https://www.rabbitmq.com/)
[![YARP](https://img.shields.io/badge/YARP-1.1.1-green)](https://microsoft.github.io/reverse-proxy/)
[![SQLite](https://img.shields.io/badge/SQLite-3.37.2-lightgrey)](https://www.sqlite.org/index.html)

## Overview

The Order Processing System is a demonstration of a microservices architecture built with ASP.NET Core, RabbitMQ, and YARP (Yet Another Reverse Proxy). This system simulates a basic e-commerce order processing workflow, demonstrating asynchronous communication and service decoupling.

This project serves as a learning platform and reference architecture for developers seeking to understand and implement microservices patterns, message queuing, and API gateways.

## Key Features

*   **Microservices Architecture:**  Well-defined, independent services with specific responsibilities.
*   **Asynchronous Communication:**  Utilizes RabbitMQ for message-based communication, enabling decoupled services.
*   **Event-Driven Architecture:**  Services react to events, promoting real-time data processing.
*   **API Gateway:** Centralized entry point using YARP for simplified routing and system management.
*   **SQLite Persistence:**  Demonstrates data storage in the Order Service (easily adaptable to other databases).
*   **.NET 7:** Built using the latest .NET technology stack.
*   **Configurable:** Easily adaptable configurations using `appsettings.json` and dependency injection.

## Services

*   **Order Service:**
    *   Handles order creation, storage, and status updates.
    *   Publishes "Order Created" events to RabbitMQ.
    *   Consumes "Payment Result" events to update order status.
    *   Uses SQLite for data persistence.
    *   Exposes RESTful API endpoints for order management.
*   **Payment Service:**
    *   Simulates payment processing (success or failure).
    *   Consumes "Order Created" events.
    *   Publishes "Payment Result" events to RabbitMQ.
*   **Notification Service:**
    *   Sends email/SMS notifications based on payment processing results.
    *   Consumes "Payment Result" events.
*   **API Gateway (YARP):**
    *   Provides a single entry point for all services.
    *   Routes requests to the appropriate backend service based on defined routes.

## Architecture Diagram

+---------------------+ +---------------------+ +---------------------+
| Order Service | | Payment Service | | Notification Service|
+---------------------+ +---------------------+ +---------------------+
| - Creates Orders | | - Processes Payments| | - Sends Emails/SMS |
| - Stores Orders | | - Publishes Results | | - Based on Payment |
| - Updates Status | | | | Results |
+---------------------+ +---------------------+ +---------------------+
| | |
| Order Created | |
-----------------> RabbitMQ Exchange ----------------->
| Payment Result | Payment Result
----------------------> RabbitMQ Exchange -------------------->

+---------------------+
| API Gateway |
+---------------------+
| - Routes Requests |
| - Provides Single |
| Entry Point |
+---------------------+


## Technologies

*   **.NET:**  7.0
*   **ASP.NET Core:**  7.0 (Web API)
*   **RabbitMQ:**  3.9+ (Message Broker)
*   **YARP (Yet Another Reverse Proxy):** 1.1.1 (API Gateway)
*   **SQLite:** 3.37.2 (Database - Order Service)
*   **Newtonsoft.Json:** (Serialization)
*   **Microsoft.Extensions.Logging:** (Logging)

## Prerequisites

*   [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
*   [RabbitMQ Server](https://www.rabbitmq.com/download.html) (Installed and running)
*   [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with the C# extension

## Setup Instructions

1.  **Clone the Repository:**

    ```bash
    git clone <repository_url>
    cd <project_directory>
    ```

2.  **Install NuGet Packages:**

    Use the .NET CLI in each project directory:

    ```bash
    # Example (OrderService):
    cd OrderService
    dotnet add package RabbitMQ.Client
    dotnet add package Microsoft.EntityFrameworkCore.Sqlite
    dotnet add package Microsoft.EntityFrameworkCore.Design
    dotnet tool install --global dotnet-ef
    cd ..

    # Run database update command to create database
    dotnet ef migrations add InitialCreate -p OrderService -s OrderService
    dotnet ef database update -p OrderService -s OrderService
    ```

    Repeat for the remaining projects, installing `RabbitMQ.Client` in `PaymentService` and `NotificationService`, and `Yarp.ReverseProxy` in `ApiGateway`.

3.  **Configure RabbitMQ:**

    *   Ensure RabbitMQ server is running and accessible.
    *   Review and configure the `RabbitMqConfig` class and `appsettings.json` files in each service, updating the RabbitMQ connection details as needed.
    *  Verify that the exchanges and queues defined in `RabbitMqConfig` exist in your RabbitMQ server. You can create them using the RabbitMQ management UI (usually accessible at `http://localhost:15672/`).

4.  **Database Configuration (Order Service):**

    *   Verify the connection string in `OrderService/appsettings.json` points to a valid SQLite database file.
    *Run database update command to create database:

    ```bash
    cd OrderService
    dotnet ef database update
    ```

5.  **API Gateway Configuration:**

    *   Edit `ApiGateway/appsettings.json` to configure the YARP reverse proxy routes and clusters, ensuring that the service addresses match where your services will be running.  Example:

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

6.  **Run the Applications:**

    Start each service (OrderService, PaymentService, NotificationService) and the ApiGateway project. You can run each project directly from your IDE (Visual Studio, VS Code) or using the .NET CLI:

    ```bash
    dotnet run
    ```

    in each service's directory.

## Usage

All requests should be routed through the API Gateway.

*   **Create Order:**

    ```
    POST https://localhost:5000/api/orders
    Content-Type: application/json

    {
      "customerId": "123",
      "totalAmount": 100.00
    }
    ```

*   **Get Order:**

    ```
    GET https://localhost:5000/api/orders/{orderId}
    ```

*   **Process Payment (Simulated):**

    ```
    POST https://localhost:5000/api/payments/process/{orderId}
    ```

## Customization

*   **RabbitMQ Configuration:**  Adjust the `RabbitMqConfig` settings in `appsettings.json` to connect to your RabbitMQ server.
*   **Database:**  Replace SQLite with a different database provider by modifying the `OrderDbContext` in the Order Service and updating the connection string.
*   **API Gateway Routing:**  Modify the `ApiGateway/appsettings.json` file to define custom routes and clusters for different services.
*   **Business Logic:** Extend the services with more complex business logic and features to create a more complete system.

## Contributing

Contributions are welcome! Please follow these guidelines:

1.  Fork the repository.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and write tests to ensure they work correctly.
4.  Submit a pull request with a clear description of your changes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

*   [RabbitMQ](https://www.rabbitmq.com/)
*   [YARP (Yet Another Reverse Proxy)](https://microsoft.github.io/reverse-proxy/)
*   [Microsoft .NET](https://dotnet.microsoft.com/)

This template provides a solid foundation for building microservices-based systems and experimenting with message queuing patterns.

## Technologies Used

*   **Language:** C# (.NET 9 or later)
*   **Framework:** ASP.NET Core Web API
*   **Message Broker:** RabbitMQ
*   **Database:** SQL Server
*   **API Gateway:** YARP (Yet Another Reverse Proxy)
*   **Serialization:** Newtonsoft.Json
*   **Logging:** Microsoft.Extensions.Logging

## Prerequisites

*   .NET 9 SDK or later
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
