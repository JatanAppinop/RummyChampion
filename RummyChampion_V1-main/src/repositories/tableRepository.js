const Table = require("../models/Table");

class tableRepository {
  async createTable(tableData) {
    tableData.pointValue = tableData.totalBet ? tableData.totalBet / 80 : 0;
    const table = new Table(tableData);
    return await table.save();
  }

  async getAllTables() {
    return await Table.find({});
  }

  async activeTables() {
    return await Table.find({ isActive: true });
  }

  async findTable(tableId) {
    return await Table.findById(tableId);
  }

  async getTables() {
    const allTables = await Table.find();
    return allTables;
  }

  async setTable(tableId, status) {
    return await Table.findByIdAndUpdate(
      { _id: tableId },
      { isActive: status }
    );
  }
}

module.exports = new tableRepository();
