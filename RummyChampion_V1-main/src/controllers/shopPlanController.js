const shopPlanService = require('../services/shopPlanService');

async function getAllShopPlans(req, res) {
    try {
        const shopPlans = await shopPlanService.getAllShopPlans();
        res.status(200).json({ success: true, data: shopPlans });
    } catch (error) {
        res.status(500).json({ success: false, message: error.message });
    }
}

async function createShopPlan(req, res) {
    try {
        const shopPlanData = req.body;
        const newShopPlan = await shopPlanService.createShopPlan(shopPlanData);
        res.status(201).json({ success: true, data: newShopPlan });
    } catch (error) {
        res.status(500).json({ success: false, message: error.message });
    }
}


module.exports = {
    getAllShopPlans, createShopPlan
};
