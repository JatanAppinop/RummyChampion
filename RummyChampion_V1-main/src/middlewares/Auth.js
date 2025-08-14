const jwt = require('../utils/Jwt');
const { JWT_SECRET } = process.env;

const Auth = async (req, res, next) => {
  try {
    const header = req.header('Authorization');
    if (header === undefined) {
      return res.status(401).json({
        success: false,
        message: "Unauthorized Request!",
        data: [],
      });
    }
    const token = header.replace('Bearer ', '');
    const user = await jwt.verifyToken(token, JWT_SECRET);
    req.user = user.data;
    return next();
  } catch (e) {
    return res.status(401).send({
      success: false,
      data: [],
      message: e.message
    });
  }
};

module.exports = {
  Auth,
};
