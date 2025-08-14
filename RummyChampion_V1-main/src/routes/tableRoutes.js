const express = require('express');
const { createTable, getAllTables, tables, tablesData,
    setAsInactive
} = require('../controllers/tableController');
const router = express.Router();
const { Auth } = require("../middlewares/Auth")

router.post('/admin/addTable', [Auth], createTable);

router.get('/admin/getTables', [Auth], getAllTables);

router.get('/user/allTables', [Auth], tables);

router.get('/user/allTablesDp', [Auth], tablesData);

router.put('/admin/setTable', [Auth], setAsInactive)

module.exports = router;
