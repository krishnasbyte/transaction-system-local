# Transaction System - Microservices on Kubernetes (Raspberry Pi 5)

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-K3s-blue)](https://k3s.io/)
[![Docker](https://img.shields.io/badge/Docker-ARM64-blue)](https://docker.com/)
[![Helm](https://img.shields.io/badge/Helm-v3-blue)](https://helm.sh/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Tests](https://github.com/krishnasbyte/transaction-system-local/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/krishnasbyte/transaction-system-local/actions)

## 📌 Overview

A complete production-grade microservices-based transaction processing system deployed on **K3s Kubernetes** running on a **Raspberry Pi 5**. This project demonstrates professional software engineering practices including containerization, orchestration, automated deployment, and comprehensive testing.

### Key Features

- ✅ **.NET 8 Microservices** (Transaction API + Background Processor)
- ✅ **Docker Containerization** optimized for ARM64 architecture
- ✅ **K3s Kubernetes** deployment on single-node Raspberry Pi
- ✅ **PostgreSQL** database with persistent storage
- ✅ **Health Checks** with liveness and readiness probes
- ✅ **Helm Charts** for easy Kubernetes deployment
- ✅ **CI/CD Ready** with GitHub Actions
- ✅ **Unit Tests** with xUnit (17 passing tests)

## 🎯 Problem Statement

Modern transaction processing systems face three key challenges:

1. **Scalability** - Handling varying transaction loads without degradation
2. **Reliability** - Ensuring no transaction is lost even during failures  
3. **Edge Deployment** - Running efficiently on resource-constrained devices

This project solves these by implementing a **decoupled microservices architecture** where transaction ingestion and processing run independently, running entirely on a Raspberry Pi 5.

## 🏗️ Architecture Diagram
┌─────────────────────────────────────────────────────────────────┐
│ K3s Kubernetes Cluster │
│ (Raspberry Pi 5) │
├─────────────────────────────────────────────────────────────────┤
│ │
│ ┌─────────────────┐ ┌─────────────────┐ │
│ │ Transaction │ │ Processor │ │
│ │ API │◄──►│ Service │ │
│ │ (2 replicas) │ │ (1 replica) │ │
│ └────────┬────────┘ └────────┬────────┘ │
│ │ │ │
│ │ ┌───────────────┘ │
│ │ │ │
│ ▼ ▼ │
│ ┌─────────────────────────────────┐ │
│ │ PostgreSQL │ │
│ │ Database │ │
│ │ (1 replica) │ │
│ └─────────────────────────────────┘ │
│ │
└─────────────────────────────────────────────────────────────────┘
│
▼
External Client (curl)

## 🧠 Design Decisions

| Decision | Why |
|----------|-----|
| **Microservices over Monolith** | Independent scaling of API and processor; fault isolation |
| **PostgreSQL over NoSQL** | ACID compliance for financial data; transaction integrity |
| **K3s over Full Kubernetes** | Lightweight (uses <512MB RAM) vs 2GB+ for full K8s; perfect for edge |
| **Background Processor** | Decouples ingestion from processing; API responds in <100ms |
| **Helm over Raw YAML** | Environment-specific configs; easy rollbacks; industry standard |

## ⚖️ Trade-offs

| Trade-off | Impact | Mitigation |
|-----------|--------|------------|
| **Single-node K3s** | No high availability | Acceptable for edge/PoC; cloud ready with AKS |
| **Shared PostgreSQL** | Services not fully decoupled | Simpler for small scale; can add message queue later |
| **No message queue** | API can get backpressure | 5-second processor polling sufficient for demo scale |

## 🔐 Security Considerations

Current implementation includes:

- ✅ Input validation on all API endpoints
- ✅ SQL injection protection via EF Core parameterized queries
- ✅ Environment variables for secrets (no hardcoded credentials)

Planned for production:

- ⏳ JWT authentication for API access
- ⏳ HTTPS/TLS with Let's Encrypt
- ⏳ Rate limiting per client

## 🚀 Why This Project Stands Out

Most transaction system demos run on cloud VMs with unlimited resources.

**This project runs on a $80 Raspberry Pi 5** while demonstrating:

- 🔹 **Edge Computing** - Full microservices on ARM64 architecture
- 🔹 **Embedded + Cloud-Native** - Rare combination of skills (C# + K3s + Docker)
- 🔹 **Production Patterns** - Health checks, Helm, CI/CD, structured logging
- 🔹 **Real Hardware** - Not a simulation; actual deployment on physical device

## 🚀 Quick Start

### Prerequisites

- Raspberry Pi 5 with Ubuntu 64-bit
- Docker installed
- K3s installed
- Helm installed

### Deploy with Helm (Recommended)

```bash
# Clone the repository
git clone https://github.com/krishnasbyte/transaction-system-local.git
cd transaction-system-local

# Build Docker images for ARM64
docker build -t transaction-api:latest -f TransactionApi/Dockerfile .
docker build -t processor-service:latest -f ProcessorService/Dockerfile .

# Load images into K3s
docker save transaction-api:latest | sudo k3s ctr images import -
docker save processor-service:latest | sudo k3s ctr images import -

# Deploy with Helm
helm install transaction-system ./helm/transaction-system \
  --namespace transactions \
  --create-namespace

# Wait for pods to be ready
kubectl wait --for=condition=ready pod -l app=transaction-api -n transactions --timeout=120s
# Test the API
bash
# Port forward to local machine
kubectl port-forward -n transactions service/transaction-api 8080:8080 &

# Health check
curl http://localhost:8080/api/transactions/health

# Create a transaction
curl -X POST http://localhost:8080/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 100.50,
    "currency": "USD",
    "sourceAccount": "ACC123",
    "destinationAccount": "ACC456"
  }'

# List all transactions
curl http://localhost:8080/api/transactions

📊 API Reference

Method	Endpoint	Description	Response
GET	/api/transactions	List all transactions (paginated)	200 OK with array
GET	/api/transactions/{id}	Get specific transaction	200 OK or 404
POST	/api/transactions	Create new transaction	201 Created
DELETE	/api/transactions/{id}	Delete transaction	204 No Content
GET	/api/transactions/health	Health check for Kubernetes	200 OK

🧪 Testing

bash
# Run all unit tests
dotnet test

# Expected output: Passed! - Failed: 0, Passed: 17, Total: 17

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

📦 Helm Commands

bash
# Install
helm install transaction-system ./helm/transaction-system -n transactions --create-namespace

# Upgrade with new values
helm upgrade transaction-system ./helm/transaction-system -n transactions --set replicaCount=3

# Rollback
helm rollback transaction-system 1 -n transactions

# Uninstall
helm uninstall transaction-system -n transactions

# View status
helm status transaction-system -n transactions
helm get values transaction-system -n transactions

🔧 Local Development

Run with Docker Compose
bash
docker compose up -d
curl http://localhost:5000/api/transactions/health
docker compose down
Run with .NET CLI
bash
# Run API
cd TransactionApi
dotnet run

# Run Processor (another terminal)
cd ProcessorService
dotnet run

📁 Project Structure

transaction-system-local/
├── TransactionApi/                 # REST API microservice
│   ├── Controllers/                # API endpoints
│   ├── Models/                     # Data models
│   ├── Data/                       # Database context
│   ├── Program.cs
│   └── Dockerfile
├── ProcessorService/               # Background worker
│   ├── Services/
│   │   └── TransactionProcessor.cs
│   ├── Program.cs
│   └── Dockerfile
├── Tests/                          # Unit tests (17 tests)
│   └── TransactionApi.Tests/
├── k8s/                            # Kubernetes manifests
│   ├── namespace.yaml
│   ├── postgres.yaml
│   ├── transaction-api.yaml
│   └── processor-service.yaml
├── helm/                           # Helm chart
│   └── transaction-system/
│       ├── Chart.yaml
│       ├── values.yaml
│       └── templates/
├── .github/workflows/              # CI/CD
│   └── dotnet-build.yml
├── docker-compose.yml
└── README.md

🔍 Monitoring & Debugging

bash
# View pods
kubectl get pods -n transactions

# View logs
kubectl logs -n transactions -l app=transaction-api
kubectl logs -n transactions -l app=processor-service -f

# Describe pod
kubectl describe pod -n transactions -l app=transaction-api

# Port forward
kubectl port-forward -n transactions service/transaction-api 8080:8080

🐛 Troubleshooting

Pods not starting
bash
kubectl describe pod -n transactions <pod-name>
kubectl logs -n transactions <pod-name> --previous

Database connection issues
bash
kubectl exec -n transactions postgres-xxx -- pg_isready
kubectl logs -n transactions postgres-xxx

Port forward not working
bash
pkill -f "kubectl port-forward"
kubectl port-forward -n transactions service/transaction-api 8081:8080

📈 Future Improvements

Add Prometheus + Grafana monitoring
Implement JWT authentication
Add Redis caching
Create message queue with RabbitMQ
Add end-to-end tests
Deploy to cloud (AKS/EKS)

👤 Author
Bikash Chhetri
Senior Software Engineer | Embedded Systems & Fintech
7+ years: C# .NET, C++, Azure, Payment Systems
Embedded + cloud-native architectures
Built: EFT-POS integrations, card issuance kiosks, RTOS payment terminals

🔗 **GitHub:** [github.com/krishnasbyte](https://github.com/krishnasbyte)  
🔗 **LinkedIn:** [linkedin.com/in/bikash-chhetri](https://linkedin.com/in/bikash-chhetri-bb2b223a9/)

📝 License
MIT License

*Built on Raspberry Pi 5 - Production-grade microservices deployment in a resource-constrained environment.*