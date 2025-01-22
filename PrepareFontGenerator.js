const ext = ["xaml", "vb", "cs"];

const fs = require('fs');
const path = require("path");
const dirname = path.resolve("./");
console.log("Initialize...");

let result = [];

const walk = function (dir) {
    let results = [];
    let list = fs.readdirSync(dir)
    let i = 0;
    (function next() {
        var file = list[i++];
        if (!file) return results;
        file = path.resolve(dir, file);
        let stat = fs.statSync(file)
        if (stat && stat.isDirectory()) {
            results = results.concat(walk(file));
            next();
        } else {
            let filename = path.basename(file);
            if (ext.indexOf(filename.substring(filename.lastIndexOf(".") + 1)) >= 0) {
                results.push(path.resolve(file));
            }
            next();
        }
    })();
    return results;
};
let arr = walk(dirname);
arr.forEach(item => {
    console.log("Processing: " + item);
    let content = fs.readFileSync(item, 'utf8');
    Array.from(content).forEach(item => {
        if (result.indexOf(item) < 0) {
            result.push(item);
        }
    });
});
result = result.sort();
console.log("Result: " + result.join(""));
fs.writeFileSync("XiaolaiSC.txt", result.join(""));