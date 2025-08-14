const User = require('../models/User');
const Transaction = require('../models/Transaction');
const Table = require('../models/Table');
const Game = require('../models/Game');

const fetchDashboardStats = async () => {
    const startOfDay = new Date();
    startOfDay.setHours(0, 0, 0, 0);

    const [
        todayCommission,
        totalCommission,
        todayGames,
        totalGames,
        todayTables,
        totalTables,
        todayDeposit,
        todayFee,
        todayWithdrawal,
        totalPlayers,
        playersPlaying,
        totalBalance
    ] = await Promise.all([
        Transaction.aggregate([
            { $match: { transactionType: 'commission', createdAt: { $gte: startOfDay } } },
            { $group: { _id: null, total: { $sum: "$amount" } } }
        ]),
        Transaction.aggregate([
            { $match: { transactionType: 'commission' } },
            { $group: { _id: null, total: { $sum: "$amount" } } }
        ]),
        Game.aggregate([
            { $match: { gameStartedDate: { $gte: startOfDay } } },
            {
                $lookup: {
                    from: 'tables',
                    localField: 'tableId',
                    foreignField: '_id',
                    as: 'table'
                }
            },
            { $unwind: '$table' },
            { $group: { _id: null, totalBet: { $sum: '$table.bet' }, count: { $sum: 1 } } }
        ]),
        Game.aggregate([
            {
                $lookup: {
                    from: 'tables',
                    localField: 'tableId',
                    foreignField: '_id',
                    as: 'table'
                }
            },
            { $unwind: '$table' },
            { $group: { _id: null, totalBet: { $sum: '$table.bet' }, count: { $sum: 1 } } }
        ]),
        Table.countDocuments({ createdAt: { $gte: startOfDay } }),
        Table.countDocuments(),
        Transaction.aggregate([
            { $match: { transactionType: 'deposit', createdAt: { $gte: startOfDay } } },
            { $group: { _id: null, total: { $sum: "$amount" } } }
        ]),
        Transaction.aggregate([
            { $match: { transactionType: 'fee', createdAt: { $gte: startOfDay } } },
            { $group: { _id: null, total: { $sum: "$amount" } } }
        ]),
        Transaction.aggregate([
            { $match: { transactionType: 'withdrawal', createdAt: { $gte: startOfDay } } },
            { $group: { _id: null, total: { $sum: "$amount" } } }
        ]),
        User.countDocuments(),
        User.countDocuments({ status: 'Active' }),
        User.aggregate([
            { $group: { _id: null, total: { $sum: "$totalBalance" } } }
        ])
    ]);

    return {
        todayCommission: todayCommission[0]?.total || 0,
        totalCommission: totalCommission[0]?.total || 0,
        todayBet: todayGames[0]?.totalBet || 0,
        totalBet: totalGames[0]?.totalBet || 0,
        todayGamesCount: todayGames[0]?.count || 0,
        totalGamesCount: totalGames[0]?.count || 0,
        todayTables,
        totalTables,
        todayDeposit: todayDeposit[0]?.total || 0,
        todayFee: todayFee[0]?.total || 0,
        todayWithdrawal: todayWithdrawal[0]?.total || 0,
        totalPlayers,
        playersPlaying,
        totalBalance: totalBalance[0]?.total || 0
    };
};


module.exports = { fetchDashboardStats };
