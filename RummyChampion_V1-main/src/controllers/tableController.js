const { func } = require('joi');
const tableService = require('../services/tableService');

async function createTable(req, res) {
    try {
        const { game, bet, totalBet, rake, rakePercentage, wonCoin, gameMode, gameType } = req.body;
        const newTable = await tableService.createTable({ game, bet, totalBet, rake, rakePercentage, wonCoin, gameMode, gameType });
        res.status(201).json({ success: true, data: newTable });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
};

async function getAllTables(req, res) {
    try {
        const tables = await tableService.getAllTables();
        res.status(200).json({ success: true, data: tables });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
};

async function tables(req, res) {
    try {
        const tables = await tableService.getUserTables();
        res.status(200).json({ success: true, data: tables });
    } catch (error) {
        res.status(400).json({ success: false, message: error.message });
    }
}

async function tablesData(req, res) {
    try {
        const data = await tableService.getTables();
        res.json({ success: true, data });
    } catch (error) {
        console.error(error);
        res.status(500).json({ success: false, message: 'Error retrieving tables' });
    }
}

async function setAsInactive(req, res) {
    const { tableId, status } = req.body;
    try {
        const data = await tableService.setTables(tableId, status);
        res.json({ success: true, data, message: 'Successfully deactivated table' })
    } catch (error) {
        res.status(500).json({ success: false, message: 'Data not updated' })
    }
}

module.exports = { createTable, getAllTables, tables, tablesData, setAsInactive }