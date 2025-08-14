const tableRepository = require('../repositories/tableRepository');

class tableService {
    async createTable(tableData) {
        return await tableRepository.createTable(tableData);
    }

    async getAllTables() {
        return await tableRepository.getAllTables();
    }

    async findTable(tableId) {
        return await tableRepository.findTable(tableId)
    }

    async getUserTables() {
        return await tableRepository.activeTables();
    }

    async getTables() {
        const allTables = await tableRepository.getTables();

        // Group tables by game mode
        const groupedTables = allTables.reduce((acc, table) => {
            const gameMode = table.gameMode;
            if (!acc[gameMode]) {
                acc[gameMode] = [];
            }
            acc[gameMode].push({ ...table._doc });
            return acc;
        }, {});

        return groupedTables;
    }

    async setTables(tableId, status) {
        const set = tableRepository.setTable(tableId, status);
        return set;
    }
}


module.exports = new tableService();