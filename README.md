# repro-marten-multitenant-singleserver-singlesession

## Usage

```bash
cd test-database
docker compose up -d
```

Make sure to run each test on it's own and clean up the database by shutting down the containers and starting them again. Database gets not cleaned up after the tests to be able to have a look at the structure and data. 

```
docker compose down && docker compose up -d
```