# Boomer
A small utility that helps clean up test data in DynamoDb

The Boomer API has 3 methods: Snapshot, Delete, Restore.

Snapshot makes a copy of the data in the database.
Delete removes all of the data in the DB.
Restore adds the data from the snapshot back into the DB.

```
[TestInitialize]
public async Task Init()
{
    await _boomer.SnapShot(_dbClient);
}

[TestCleanup]
public async Task Cleanup()
{
    await _boomer.Delete(_dbClient);
    await _boomer.Restore(_dbClient);
}
```
