const express = require('express');
const { getAllShopPlans, createShopPlan } = require('../controllers/shopPlanController');
const router = express.Router();
const { Auth } = require("../middlewares/Auth")

router.get('/admin/getShopPlans', [Auth], getAllShopPlans);

router.post('/admin/addShopPlans', [Auth], createShopPlan);

module.exports = router;
