const ShopPlan = require('../models/ShopPlan');

class ShopPlanRepository {
    async getAllShopPlans() {
        return await ShopPlan.find();
    }

    async createShopPlan(shopPlanData) {
        const shopPlan = new ShopPlan(shopPlanData);
        return await ShopPlan.create(shopPlanData);
    }

    async getShopPlans() {
        return await ShopPlan.find({ isActive: true }).sort({ createdOn: -1 });
    }

    async getShopPlanById(planId) {
        return await ShopPlan.findById(planId);
    }

}
module.exports = new ShopPlanRepository();
