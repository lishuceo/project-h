/**
 * 初始化测试数据（仅用于开发测试）
 */

export function initTestData(): void {
  // 设置一个测试最高分
  if (!localStorage.getItem('highest_score')) {
    localStorage.setItem('highest_score', '500');
    console.log('已设置测试最高分: 500');
  }
}

